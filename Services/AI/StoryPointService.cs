using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using Tokenizers.HuggingFace.Tokenizer;

public class StoryPointService
{
    private InferenceSession _sbertSession;
    private InferenceSession _spSession;
    private Tokenizer _tokenizer;

    public async Task InitializeAsync()
    {
        string sbert = await GetAssetPathAsync("ML/model.onnx");
        string sp = await GetAssetPathAsync("ML/storypoint_model.onnx");
        string tok = await GetAssetPathAsync("ML/tokenizer.json");

        _sbertSession = new InferenceSession(sbert);
        _spSession = new InferenceSession(sp);
        _tokenizer = Tokenizer.FromFile(tok);
    }

    private async Task<string> GetAssetPathAsync(string assetPath)
    {
        string target = Path.Combine(FileSystem.AppDataDirectory, assetPath);

        // Ensure directory exists
        Directory.CreateDirectory(Path.GetDirectoryName(target)!);

        if (!File.Exists(target))
        {
            using var stream = await FileSystem.OpenAppPackageFileAsync(assetPath);
            using var outFile = File.Create(target);
            await stream.CopyToAsync(outFile);
        }

        return target;
    }

    public float Predict(string story)
    {
        float[] embedding = Embed(story);

        float prediction = PredictStoryPoints(embedding);

        return ToFibonacci(prediction);
    }

    private float ToFibonacci(float n)
    {
        float[] fib = { 1, 2, 3, 5, 8, 13 };
        float closest = fib[0];
        float minDiff = Math.Abs(fib[0] - n);

        for (int i = 1; i < fib.Length; i++)
        {
            float diff = Math.Abs(fib[i] - n);
            if (diff < minDiff)
            {
                minDiff = diff;
                closest = fib[i];
            }
        }

        return closest;
    }

    private float[] Embed(string text)
    {
        // 1. Tokenize using HuggingFace tokenizer
        var enc = _tokenizer.Encode(
            input: text,
            addSpecialTokens: true,
            input2: null,
            includeTypeIds: false,
            includeTokens: false,
            includeWords: false,
            includeOffsets: false,
            includeSpecialTokensMask: false,
            includeAttentionMask: true,
            includeOverflowing: false
        ).First();

        long[] inputIds = enc.Ids.Select(i => (long)i).ToArray();
        long[] attentionMask = enc.AttentionMask.Select(i => (long)i).ToArray();
        int seqLen = inputIds.Length;

        // 2. Build tensors
        var idTensor = new DenseTensor<long>(inputIds, new[] { 1, seqLen });
        var maskTensor = new DenseTensor<long>(attentionMask, new[] { 1, seqLen });

        // 3. ONNX inference
        using var outputs = _sbertSession.Run(new[]
        {
        NamedOnnxValue.CreateFromTensor("input_ids", idTensor),
        NamedOnnxValue.CreateFromTensor("attention_mask", maskTensor)
    });

        float[] hidden = outputs.First().AsEnumerable<float>().ToArray();

        int dim = 768;
        float[] pooled = new float[dim];
        float validCount = 0f;

        // 4. Mean pooling with mask (MATCHES SENTENCE-TRANSFORMERS)
        for (int i = 0; i < seqLen; i++)
        {
            if (attentionMask[i] == 0)
                continue;

            var span = new ReadOnlySpan<float>(hidden, i * dim, dim);

            for (int j = 0; j < dim; j++)
                pooled[j] += span[j];

            validCount++;
        }

        for (int j = 0; j < dim; j++)
            pooled[j] /= validCount;

        // 5. L2 Normalize (KEY DIFFERENCE)
        float norm = 0f;
        for (int j = 0; j < dim; j++)
            norm += pooled[j] * pooled[j];
        norm = (float)Math.Sqrt(norm);

        for (int j = 0; j < dim; j++)
            pooled[j] /= norm;

        return pooled;
    }

    private float PredictStoryPoints(float[] embedding)
    {
        var embTensor = new DenseTensor<float>(embedding, new[] { 1, embedding.Length });

        using var outputs = _spSession.Run(new[]
        {
            NamedOnnxValue.CreateFromTensor("input", embTensor)
        });

        return outputs.First().AsEnumerable<float>().First();
    }

}
