using CommunityToolkit.Maui.Storage;
using Microsoft.Extensions.Logging;
using NanoXLSX;
using MAPSAI.Models;
using MAPSAI.Services.Builders;
using SpreadCheetah;
using System.Collections.ObjectModel;
using System.Diagnostics;
using Cell = SpreadCheetah.Cell;
using MAPSAI.Core.Models;

namespace MAPSAI.Services.Files
{

    public class ExcelService
    {
        private readonly ILogger<ExcelService> _logger;

        public ExcelService(ILogger<ExcelService> logger) 
        {
            _logger = logger;
        }

        public ExcelResponse ParseProcessExcel(string excelPath)
        {
            var nodes = new ObservableCollection<UserStory>();

            try
            {
                Workbook wb = Workbook.Load(excelPath);
                var sheet = wb.CurrentWorksheet;
                _logger.LogInformation("Loaded workbook. Active sheet: {SheetName}", sheet.SheetName);

                int headerRow = 0; // NO HEADER USED

                for (int row = headerRow; row <= sheet.GetLastDataRowNumber(); row++)
                {
                    string idStr = "";

                    if (sheet.HasCell(0, row))
                    {
                        idStr = sheet.GetCell(0, row)?.Value?.ToString()?.Trim() ?? "";
                        if (string.IsNullOrWhiteSpace(idStr))
                        {
                            return new ExcelResponse(false, "All ID's in A Column should have a valid value!", null, null);
                        }
                    }
                    else 
                    {
                        return new ExcelResponse(false, "All ID's in A Column should have a valid value!", null, null);
                    }

                    string story = "";

                    if (sheet.HasCell(1, row) )
                    {
                        story = sheet.GetCell(1, row)?.Value?.ToString()?.Trim() ?? "";
                        if (string.IsNullOrWhiteSpace(story))
                        {
                            return new ExcelResponse(false, "All Stories in B Column should have a valid value!", null, null);
                        }
                    }
                    else
                    {
                        return new ExcelResponse(false, "All Stories in B Column should have a valid value!", null, null);
                    }
                    
                    string actor = "";

                    if (sheet.HasCell(2, row))
                    {
                        actor = sheet.GetCell(2, row)?.Value?.ToString()?.Trim() ?? "";
                        if (string.IsNullOrWhiteSpace(actor))
                        {
                            return new ExcelResponse(false, "All Actors in C Column should have a valid value!", null, null);
                        }
                    }
                    else
                    {
                        return new ExcelResponse(false, "All Actors in C Column should have a valid value!", null, null);
                    }

                    string connStr = sheet.HasCell(3, row) ? sheet.GetCell(3, row).Value?.ToString()?.Trim() ?? "" : "";

                    if (string.IsNullOrWhiteSpace(idStr))
                        continue;

                    List<string> connections = connStr
                        .Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .ToList();

                    var conns = new ObservableCollection<Connection>(); 

                    foreach (string connection in connections)
                    {
                        var con = new Connection(idStr, idStr, connection) { };

                        conns.Add(con);
                    }

                    nodes.Add(new UserStory()
                    {
                        ID = idStr,
                        Story = story,
                        User = actor,
                        Connections = conns,
                    });
                }

                return new ExcelResponse(true, "Successfully parsed Excel file.", null, nodes) { };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while parsing story Excel file.");
                return new ExcelResponse(false, "Error occurred while parsing Excel!", null, null);
            }
        }

        public ExcelResponse ParseExcel(string excelPath)
        {
            try
            {
                Workbook wb = Workbook.Load(excelPath);

                _logger.LogInformation("Loaded workbook. Active sheet: {SheetName}", wb.CurrentWorksheet.SheetName);

                var resDict = new Dictionary<string, string>();

                foreach (var cell in wb.CurrentWorksheet.Cells)
                {
                    string cellColumn = cell.Key.Substring(0, 1);
                    string cellRow = cell.Key.Substring(1);

                    var cellValue = cell.Value is not null && cell.Value.Value is not null ? cell.Value.Value.ToString() : string.Empty;

                    if (float.Parse(cellRow) == 1)
                    {
                        foreach (var innerCell in wb.CurrentWorksheet.Cells)
                        {
                            if (innerCell.Key.Substring(0, 1) == cellColumn && float.Parse(innerCell.Key.Substring(1)) == 2)
                            {
                                resDict.Add(cellValue, innerCell.Value.ToString() is not null && innerCell.Value.Value.ToString() is not null ? innerCell.Value.Value.ToString() : string.Empty);
                            }
                        }
                    }
                }
                return new ExcelResponse(true, null, resDict, null);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                return new ExcelResponse(false, "Error occured while parsing excel!", null, null);
            }
        }

        public int GetAlphabetPosition(string letter)
        {
            if (string.IsNullOrEmpty(letter) || letter.Length != 1)
                throw new ArgumentException("Input must be a single letter.");

            char c = char.ToUpper(letter[0]);

            if (c < 'A' || c > 'Z')
                throw new ArgumentException("Input must be a letter from A-Z.");

            return c - 'A' + 1;
        }

        private void LogTree(TreeNode<string> node, string indent = "")
        {
            Debug.WriteLine($"{indent}{node.Value} (Instr: {node.Instruction})");
            foreach (var child in node.Children)
            {
                LogTree(child, indent + "  ");
            }
        }

        public TreeNode<string> BuildTreeFromExcel(string excelPath)
        {
            try
            {
                Workbook wb = Workbook.Load(excelPath);
                var worksheet = wb.Worksheets[0];

                var root = new TreeNode<string>("Root");
                var stack = new Stack<TreeNode<string>>();
                stack.Push(root);

                // Track cells that are instructions so we don't treat them as main nodes
                var instructionKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                foreach (var cell in worksheet.Cells
                    .OrderBy(c => int.Parse(new string(c.Key.SkipWhile(char.IsLetter).ToArray())))
                    .ThenBy(c => GetAlphabetPosition(new string(c.Key.TakeWhile(char.IsLetter).ToArray()))))
                {
                    if (instructionKeys.Contains(cell.Key))
                        continue;

                    string cellKey = cell.Key;
                    string colLetter = new string(cellKey.TakeWhile(char.IsLetter).ToArray());
                    string rowNumberString = new string(cellKey.SkipWhile(char.IsLetter).ToArray());

                    int currentCol = GetAlphabetPosition(colLetter);
                    int currentRow = int.Parse(rowNumberString);
                    string cellValue = cell.Value?.Value?.ToString();

                    if (string.IsNullOrWhiteSpace(cellValue))
                        continue;

                    string nextColLetter = ((char)('A' + currentCol)).ToString();
                    string rightCellKey = $"{nextColLetter}{currentRow}";

                    worksheet.Cells.TryGetValue(rightCellKey, out var rightCell);
                    string instruction = rightCell?.Value?.ToString();

                    if (!string.IsNullOrWhiteSpace(instruction))
                        instructionKeys.Add(rightCellKey);

                    while (stack.Count > currentCol)
                        stack.Pop();

                    var newNode = new TreeNode<string>(cellValue, instruction);
                    stack.Peek().AddChild(newNode);
                    stack.Push(newNode);
                }

                LogTree(root);
                return root;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                return new TreeNode<string>("", "");
            }
           
        }

        public async Task<ExcelResponse> DictionaryToExcelAsync(Dictionary<string, string> data, string filename)
        {
            try
            {
                using var stream = new MemoryStream();

                await using (var spreadsheet = await Spreadsheet.CreateNewAsync(stream))
                {
                    await spreadsheet.StartWorksheetAsync("Data");

                    foreach (var kvp in data)
                    {
                        var row = new List<Cell>
                {
                    new(kvp.Key),
                    new(kvp.Value)
                };
                        await spreadsheet.AddRowAsync(row);
                    }

                    await spreadsheet.FinishAsync();
                }

                stream.Position = 0;

                if (Application.Current?.Windows.LastOrDefault()?.Handler.PlatformView is Microsoft.UI.Xaml.Window window)
                {
                    window.Activate();
                }

                var fileSaverResult = await FileSaver.Default.SaveAsync(
                    $"{filename}.xlsx",
                    stream,
                    CancellationToken.None);

                if (fileSaverResult.IsSuccessful)
                {
                    return new ExcelResponse(true, null, null, null);
                }
                else
                {
                    return new ExcelResponse(false, fileSaverResult.Exception?.Message ?? "Save canceled", null, null);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                return new ExcelResponse(false, ex.Message, null, null);
            }
        }
    }

}
