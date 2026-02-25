using CommunityToolkit.Maui.Storage;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using MAPSAI.Models;
using MAPSAI.Services.Files.Models;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using Style = DocumentFormat.OpenXml.Wordprocessing.Style;

namespace MAPSAI.Services.Files
{

    public class MSWordService
    {
        public MSWordService() { }

        public async Task<DocumentResponse> GenerateDocument(
            Collection<TreeNode<string>> data,
            string title,
            string fileName)
        {
            try
            {
                string tempPath = Path.Combine(Path.GetTempPath(), $"{fileName}.docx");

                await Task.Run(() =>
                {
                    if (File.Exists(tempPath)) File.Delete(tempPath);

                    using (var wordDoc = WordprocessingDocument.Create(tempPath, WordprocessingDocumentType.Document))
                    {
                        var mainPart = wordDoc.AddMainDocumentPart();
                        mainPart.Document = new Document();
                        var body = new Body();

                        var sectPr = new SectionProperties(
                            new PageSize { Width = 11906, Height = 16838 }, // A4
                            new PageMargin
                            {
                                Top = 1440,
                                Bottom = 1440,
                                Left = 1440,
                                Right = 1440,
                                Header = 720,
                                Footer = 720,
                                Gutter = 0
                            });

                        EnsureFooterWithPageNumbers(mainPart, body, pageXofY: true);

                        mainPart.Document.Append(body);
                        AddStyles(mainPart);

                        int bulletNumberingId = EnsureBulletNumbering(mainPart);

                        body.Append(CreateParagraph(title, "Title"));
                        AddTableOfContents(body);
                        body.Append(new Paragraph(new Run(new Break { Type = BreakValues.Page })));

                        bool firstLevel1Seen = false;

                        foreach (var section in FlattenPreOrder(data))
                        {
                            // (Optional) If you only want active nodes:
                            // if (!section.IsActive) continue;

                            string content = CleanSectionContent(section);

                            if (section.Level == 1)
                            {
                                if (firstLevel1Seen)
                                    body.Append(new Paragraph(new Run(new Break { Type = BreakValues.Page })));

                                firstLevel1Seen = true;
                            }

                            string headingStyle = section.Level switch
                            {
                                1 => "Heading1",
                                2 => "Heading2",
                                _ => "Heading3"
                            };

                            body.Append(CreateParagraph(section.Value, headingStyle));

                            if (IsMarkdownTable(content))
                                AppendMarkdownTable(body, content);
                            else if (ContainsBulletList(content))
                                AppendBulletList(body, content, bulletNumberingId);
                            else
                                body.Append(CreateParagraph(content, "Normal"));
                        }

                        EnableAutoUpdateFields(mainPart);

                        // Ensure SectionProperties exists at end of body
                        if (body.Elements<SectionProperties>().LastOrDefault() == null)
                            body.Append(sectPr);

                        mainPart.Document.Save();
                    } // <-- CRITICAL: closes docx package here
                });

                // Now file is fully closed; safe to read
                using var fileStream = File.OpenRead(tempPath);
                using var stream = new MemoryStream();
                await fileStream.CopyToAsync(stream);
                stream.Position = 0;

                // Activate window (optional)
                if (Application.Current?.Windows.LastOrDefault()?.Handler?.PlatformView is Microsoft.UI.Xaml.Window window)
                    window.Activate();

                // Picker must be on UI thread
                var fileSaverResult = await MainThread.InvokeOnMainThreadAsync(() =>
                    FileSaver.Default.SaveAsync($"{fileName}.docx", stream, CancellationToken.None)
                );

                return new DocumentResponse
                {
                    Success = fileSaverResult.IsSuccessful,
                    Error = fileSaverResult.Exception?.Message ?? (fileSaverResult.IsSuccessful ? null : "Save canceled")
                };
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                return new DocumentResponse { Success = false, Error = ex.Message };
            }
        }

        private static IEnumerable<TreeNode<string>> FlattenPreOrder(IEnumerable<TreeNode<string>> roots)
        {
            foreach (var root in roots)
            {
                foreach (var n in FlattenPreOrder(root))
                    yield return n;
            }
        }

        private static IEnumerable<TreeNode<string>> FlattenPreOrder(TreeNode<string> node)
        {
            yield return node;

            if (node.Children == null)
                yield break;

            foreach (var child in node.Children)
            {
                foreach (var n in FlattenPreOrder(child))
                    yield return n;
            }
        }

        private void EnsureFooterWithPageNumbers(MainDocumentPart mainPart, Body body, bool pageXofY)
        {
            // 1) Create footer part
            var footerPart = mainPart.AddNewPart<FooterPart>();
            string footerPartId = mainPart.GetIdOfPart(footerPart);

            // 2) Build footer content
            var footer = new Footer();

            var p = new Paragraph();
            var pPr = new ParagraphProperties(
                new Justification() { Val = JustificationValues.Center },
                new SpacingBetweenLines() { Before = "0", After = "0" }
            );
            p.Append(pPr);

            if (pageXofY)
            {
                // "Page " + PAGE + " of " + NUMPAGES
                p.Append(new Run(new Text("Page ") { Space = SpaceProcessingModeValues.Preserve }));

                p.Append(new Run(
                    new FieldChar() { FieldCharType = FieldCharValues.Begin }
                ));
                p.Append(new Run(
                    new FieldCode(" PAGE ") { Space = SpaceProcessingModeValues.Preserve }
                ));
                p.Append(new Run(
                    new FieldChar() { FieldCharType = FieldCharValues.End }
                ));

                p.Append(new Run(new Text(" of ") { Space = SpaceProcessingModeValues.Preserve }));

                p.Append(new Run(
                    new FieldChar() { FieldCharType = FieldCharValues.Begin }
                ));
                p.Append(new Run(
                    new FieldCode(" NUMPAGES ") { Space = SpaceProcessingModeValues.Preserve }
                ));
                p.Append(new Run(
                    new FieldChar() { FieldCharType = FieldCharValues.End }
                ));
            }
            else
            {
                // Just PAGE
                p.Append(new Run(
                    new FieldChar() { FieldCharType = FieldCharValues.Begin }
                ));
                p.Append(new Run(
                    new FieldCode(" PAGE ") { Space = SpaceProcessingModeValues.Preserve }
                ));
                p.Append(new Run(
                    new FieldChar() { FieldCharType = FieldCharValues.End }
                ));
            }

            footer.Append(p);
            footerPart.Footer = footer;
            footerPart.Footer.Save();

            // 3) Get or create section properties (MUST be the last in body)
            var sectPr = body.Elements<SectionProperties>().LastOrDefault();
            if (sectPr == null)
            {
                sectPr = new SectionProperties();
                body.Append(sectPr);
            }

            // 4) Attach footer reference
            // Remove any existing default footer reference to avoid duplicates
            foreach (var existing in sectPr.Elements<FooterReference>()
                         .Where(fr => fr.Type == HeaderFooterValues.Default).ToList())
            {
                existing.Remove();
            }

            sectPr.Append(new FooterReference()
            {
                Type = HeaderFooterValues.Default,
                Id = footerPartId
            });

            // Ensure margins reserve footer space (yours already do, but safe)
            var margin = sectPr.GetFirstChild<PageMargin>();
            if (margin == null)
            {
                sectPr.PrependChild(new PageMargin() { Footer = 720, Header = 720, Top = 1440, Bottom = 1440, Left = 1440, Right = 1440 });
            }
            else
            {
                if (margin.Footer == null) margin.Footer = 720;
                if (margin.Header == null) margin.Header = 720;
            }
        }

        // =========================================================
        // STYLING
        // =========================================================

        private void AddStyles(MainDocumentPart mainPart)
        {
            var stylesPart = mainPart.AddNewPart<StyleDefinitionsPart>();
            stylesPart.Styles = new Styles(
                new Style(
                    new Name() { Val = "Title" },
                    new BasedOn() { Val = "Normal" },
                    new NextParagraphStyle() { Val = "Normal" },
                    new UIPriority() { Val = 10 },
                    new PrimaryStyle(),
                    new StyleRunProperties(
                        new Bold(),
                        new FontSize() { Val = "40" } // 20pt
                    )
                )
                { Type = StyleValues.Paragraph, StyleId = "Title" },

                new Style(
                    new Name() { Val = "Heading 1" },
                    new BasedOn() { Val = "Normal" },
                    new UIPriority() { Val = 9 },
                    new PrimaryStyle(),
                    new StyleRunProperties(
                        new Bold(),
                        new FontSize() { Val = "28" } // 14pt
                    )
                )
                { Type = StyleValues.Paragraph, StyleId = "Heading1" },

                new Style(
                    new Name() { Val = "Heading 2" },
                    new BasedOn() { Val = "Normal" },
                    new StyleRunProperties(
                        new Bold(),
                        new FontSize() { Val = "24" }
                    )
                )
                { Type = StyleValues.Paragraph, StyleId = "Heading2" },

                new Style(
                    new Name() { Val = "Heading 3" },
                    new BasedOn() { Val = "Normal" },
                    new StyleRunProperties(
                        new Bold(),
                        new FontSize() { Val = "22" }
                    )
                )
                { Type = StyleValues.Paragraph, StyleId = "Heading3" }
            );
        }

        // =========================================================
        // TOC
        // =========================================================

        private void AddTableOfContents(Body body)
        {
            var paragraph = new Paragraph();

            // Begin field
            paragraph.Append(
                new Run(
                    new FieldChar() { FieldCharType = FieldCharValues.Begin }
                )
            );

            // Field code
            paragraph.Append(
                new Run(
                    new FieldCode(" TOC \\o \"1-3\" \\h \\z \\u ")
                    {
                        Space = SpaceProcessingModeValues.Preserve
                    }
                )
            );

            // Separate (required!)
            paragraph.Append(
                new Run(
                    new FieldChar() { FieldCharType = FieldCharValues.Separate }
                )
            );

            // Placeholder text (what Word shows before update)
            paragraph.Append(
                new Run(
                    new Text("Table of Contents - Right click and Update Field.")
                )
            );

            // End field
            paragraph.Append(
                new Run(
                    new FieldChar() { FieldCharType = FieldCharValues.End }
                )
            );

            body.Append(paragraph);
        }

        // =========================================================
        // PARAGRAPH CREATION
        // =========================================================

        private Paragraph CreateParagraph(string text, string style)
        {
            var paragraph = new Paragraph();
            var properties = new ParagraphProperties();

            properties.ParagraphStyleId = new ParagraphStyleId() { Val = style };

            // Justify only normal paragraphs
            if (style == "Normal")
            {
                properties.Append(new Justification() { Val = JustificationValues.Both });
            }

            paragraph.Append(properties);

            var run = new Run();
            run.Append(new Text(text)
            {
                Space = SpaceProcessingModeValues.Preserve
            });

            paragraph.Append(run);
            return paragraph;
        }

        // =========================================================
        // TABLE SUPPORT
        // =========================================================

        private bool IsMarkdownTable(string content)
        {
            return Regex.IsMatch(content, @"^\s*\|.+\|\s*$", RegexOptions.Multiline);
        }

        bool IsSeparatorRow(string line)
        {
            var t = line.Replace("|", "").Trim();
            return Regex.IsMatch(t, @"^:?-{3,}:?(?:\s*:?-{3,}:?)*\s*$");
        }

        private void EnableAutoUpdateFields(MainDocumentPart mainPart)
        {
            var settingsPart = mainPart.AddNewPart<DocumentSettingsPart>();
            settingsPart.Settings = new Settings(
                new UpdateFieldsOnOpen() { Val = true }
            );
        }

        private void AppendMarkdownTable(Body body, string content)
        {
            var lines = content.Split('\n')
                .Where(l => l.Trim().StartsWith("|"))
                .ToList();

            // Remove the markdown separator row (|---|---|)
            lines = lines.Where(l => !IsSeparatorRow(l)).ToList();

            if (lines.Count < 1)
                return;

            var table = new Table();

            // ===== TABLE BORDERS =====
            var tableProperties = new TableProperties(
                new TableBorders(
                    new TopBorder
                    {
                        Val = BorderValues.Single,
                        Size = 4, // 4 = 0.5pt (size is in eighths of a point)
                        Color = "000000"
                    },
                    new BottomBorder
                    {
                        Val = BorderValues.Single,
                        Size = 4,
                        Color = "000000"
                    },
                    new LeftBorder
                    {
                        Val = BorderValues.Single,
                        Size = 4,
                        Color = "000000"
                    },
                    new RightBorder
                    {
                        Val = BorderValues.Single,
                        Size = 4,
                        Color = "000000"
                    },
                    new InsideHorizontalBorder
                    {
                        Val = BorderValues.Single,
                        Size = 4,
                        Color = "000000"
                    },
                    new InsideVerticalBorder
                    {
                        Val = BorderValues.Single,
                        Size = 4,
                        Color = "000000"
                    }
                )
            );

            table.AppendChild(tableProperties);

            // ===== TABLE CONTENT =====
            foreach (var line in lines)
            {
                var row = new TableRow();
                var cells = line.Trim('|').Split('|');

                foreach (var cellText in cells)
                {
                    var cell = new TableCell(
                        new Paragraph(
                            new Run(
                                new Text(cellText.Trim())
                                {
                                    Space = SpaceProcessingModeValues.Preserve
                                }
                            )
                        )
                    );

                    row.Append(cell);
                }

                table.Append(row);
            }

            body.Append(table);
        }


        // =========================================================
        // BULLET LIST SUPPORT
        // =========================================================

        private static string NormalizeInlineBullets(string content)
        {
            if (string.IsNullOrWhiteSpace(content))
                return content;

            content = content.Replace("\r\n", "\n").Replace("\r", "\n");
            content = content.Replace("\t", "    ");

            content = Regex.Replace(content, @"(?<!\n)\s{2,}-\s+(?=\S)", "\n- ");
            content = Regex.Replace(content, @"^\s*-\s*", "- ");

            return content;
        }

        private bool ContainsBulletList(string content)
        {
            return Regex.IsMatch(content, @"^\s*-\s+", RegexOptions.Multiline);
        }

        private void AppendBulletList(Body body, string content, int numberingId)
        {
            content = NormalizeInlineBullets(content);

            var lines = content.Split('\n')
                .Select(l => l.Replace("\r", ""))
                .Where(l => !string.IsNullOrWhiteSpace(l))
                .Where(l => l.TrimStart().StartsWith("-"))
                .ToList();

            // Heuristic: if there is no indentation anywhere, infer levels
            bool hasIndent = lines.Any(l => l.TakeWhile(c => c == ' ').Count() >= 2);

            int currentRoleLevel = 0;     // 0
            int currentFeatureLevel = 1;  // 1
            int criteriaLevel = 2;        // 2

            foreach (var rawLine in lines)
            {
                int level;

                int leadingSpaces = rawLine.TakeWhile(c => c == ' ').Count();

                string trimmed = rawLine.TrimStart();
                trimmed = trimmed.Substring(1).Trim(); // remove '-' then trim

                if (leadingSpaces >= 2)
                {
                    // Original indentation-based nesting (2 spaces per level)
                    level = Math.Clamp(leadingSpaces / 2, 0, 3);
                }
                else
                {
                    // No indentation -> infer
                    if (Regex.IsMatch(trimmed, @"^\*\*.+\*\*(\s*:)?\s*$"))
                    {
                        level = currentRoleLevel; // Role header
                    }
                    else if (LooksLikeFeatureTitle(trimmed))
                    {
                        level = currentFeatureLevel; // Feature / action
                    }
                    else
                    {
                        level = criteriaLevel; // Acceptance criteria / outcomes
                    }
                }

                var paragraph = new Paragraph();

                var pPr = new ParagraphProperties(
                    new NumberingProperties(
                        new NumberingLevelReference() { Val = level },
                        new NumberingId() { Val = numberingId }
                    ),
                    new SpacingBetweenLines() { Before = "0", After = "60" }
                );

                paragraph.Append(pPr);

                // Bold handling **...**
                var parts = Regex.Split(trimmed, @"(\*\*.*?\*\*)");
                foreach (var part in parts)
                {
                    if (string.IsNullOrEmpty(part)) continue;

                    Run run;
                    if (part.StartsWith("**") && part.EndsWith("**"))
                    {
                        string boldText = part.Substring(2, part.Length - 4);
                        run = new Run(
                            new RunProperties(new Bold()),
                            new Text(boldText) { Space = SpaceProcessingModeValues.Preserve }
                        );
                    }
                    else
                    {
                        run = new Run(new Text(part) { Space = SpaceProcessingModeValues.Preserve });
                    }

                    paragraph.Append(run);
                }

                body.Append(paragraph);
            }
        }

        private static bool LooksLikeFeatureTitle(string text)
        {
            // Typical imperative feature verbs (extend as needed)
            return Regex.IsMatch(
                text,
                @"^(Add|Create|Update|Delete|Release|Reject|Approve|Review|View|Choose|Select|Submit|Discuss|Accept|Assign|Notify|Manage|Maintain)\b",
                RegexOptions.IgnoreCase
            );
        }

        private int EnsureBulletNumbering(MainDocumentPart mainPart)
        {
            var numberingPart = mainPart.NumberingDefinitionsPart;
            if (numberingPart == null)
                numberingPart = mainPart.AddNewPart<NumberingDefinitionsPart>();

            if (numberingPart.Numbering == null)
                numberingPart.Numbering = new Numbering();

            int abstractNumId = numberingPart.Numbering.Elements<AbstractNum>().Any()
                ? numberingPart.Numbering.Elements<AbstractNum>().Max(a => (int)a.AbstractNumberId.Value) + 1
                : 1;

            int numId = numberingPart.Numbering.Elements<NumberingInstance>().Any()
                ? numberingPart.Numbering.Elements<NumberingInstance>().Max(n => (int)n.NumberID.Value) + 1
                : 1;

            // ---- Define bullet style levels (0..3)
            var abstractNum = new AbstractNum() { AbstractNumberId = abstractNumId };

            for (int level = 0; level <= 3; level++)
            {
                int left = 720 + (level * 360);
                const int hanging = 360;

                abstractNum.Append(
                    new Level(
                        new NumberingFormat() { Val = NumberFormatValues.Bullet },
                        new LevelText() { Val = "•" },
                        new LevelJustification() { Val = LevelJustificationValues.Left },
                        new ParagraphProperties(
                            new Indentation()
                            {
                                Left = left.ToString(),
                                Hanging = hanging.ToString()
                            }
                        ),
                        new RunProperties(
                            new RunFonts() { Ascii = "Calibri", HighAnsi = "Calibri" }
                        )
                    )
                    { LevelIndex = level }
                );
            }

            numberingPart.Numbering.Append(abstractNum);

            // ---- IMPORTANT: create the numbering instance that your paragraphs reference
            var instance = new NumberingInstance(
                new AbstractNumId() { Val = abstractNumId }
            )
            { NumberID = numId };

            numberingPart.Numbering.Append(instance);

            numberingPart.Numbering.Save();

            return numId;
        }


        // =========================================================
        // CONTENT CLEANUP
        // =========================================================

        private string CleanSectionContent(TreeNode<string> section)
        {
            string content = section.Content ?? "";

            int index = content.IndexOf(section.Value, StringComparison.OrdinalIgnoreCase);

            if (index >= 0)
                content = content.Remove(index, section.Value.Length);

            content = content.Replace("MODE A (WRITE)", "");

            return content.Trim();
        }

    }
}
