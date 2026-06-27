using MurdoxV2.Enums;
using MurdoxV2.Hashing;
using SkiaSharp;

namespace MurdoxV2.Features.ScamDetection
{
    public sealed class HeuristicScamDetectionService(ScamDetectionConfig config, ScamImageHashConfig hashConfig, IScamHashRepository repository) : IScamDetectionService
    {
        private readonly ScamDetectionConfig _config = config;
        private readonly ScamImageHashConfig _hashConfig = hashConfig;
        private readonly IScamHashRepository _repository = repository;

        public async Task<ScamAnalysisResult> AnalyzeAsync(ScamImageContext context, byte[] imageBytes)
        {
            // 1. Compute perceptual hashes
            using var bitmap = SKBitmap.Decode(imageBytes);
            if (bitmap is null)
                return new ScamAnalysisResult(ScamVerdict.Clean, "Invalid or unreadable image", 0f);

            long aHash = ImageHashing.AverageHash(bitmap);
            long dHash = ImageHashing.DifferenceHash(bitmap);
            long pHash = ImageHashing.PerceptualHash(bitmap);

            // 2. Load known scam hashes
            var known = await _repository.GetAllAsync();
            if (!known.Any())
                return new ScamAnalysisResult(ScamVerdict.Clean, "No known scam images in database", 0f);

            ScamImageRecord? bestMatch = null;
            int bestScore = int.MaxValue;

            foreach (var record in known)
            {
                int aDist = HammingDistance(aHash, record.AHash);
                int dDist = HammingDistance(dHash, record.DHash);
                int pDist = HammingDistance(pHash, record.PHash);

                // Weighted score (pHash strongest)
                int weighted = (pDist * 3) + (dDist * 2) + (aDist * 1);

                if (weighted < bestScore)
                {
                    bestScore = weighted;
                    bestMatch = record;
                }
            }

            if (bestMatch is null)
                return new ScamAnalysisResult(ScamVerdict.Clean, "No match found", 0f);

            // 3. Compute distances for the best match
            int aDiff = HammingDistance(aHash, bestMatch.AHash);
            int dDiff = HammingDistance(dHash, bestMatch.DHash);
            int pDiff = HammingDistance(pHash, bestMatch.PHash);

            // 4. Determine verdict
            ScamVerdict verdict;
            string reason;

            // Realistic pHash thresholds
            int A_STRICT = _hashConfig.AHashThreshold;
            int D_STRICT = _hashConfig.DHashThreshold;
            int P_STRICT = _hashConfig.PHashThreshold;

            int A_LOOSE = A_STRICT + 6;
            int D_LOOSE = D_STRICT + 6;
            int P_LOOSE = P_STRICT + 6;

            if (pDiff <= P_STRICT && dDiff <= D_STRICT && aDiff <= A_STRICT)
            {
                verdict = ScamVerdict.Scam;
                reason = $"Exact or near-exact match to known scam image (pHash: {pDiff})";
            }
            else if ((pDiff <= P_LOOSE && dDiff <= D_LOOSE) ||
                    (pDiff <= P_LOOSE && aDiff <= A_LOOSE) ||
                    (dDiff <= D_LOOSE && aDiff <= A_LOOSE)) 
            {
                verdict = ScamVerdict.Suspicious;
                reason = $"Image is visually similar to known scam templates (pHash: {pDiff})";
            }
            else
            {
                verdict = ScamVerdict.Clean;
                reason = "No significant similarity to known scam images";
            }


            // 5. Convert distances into a 0–1 score
            float score = CalculateScore(aDiff, dDiff, pDiff);

            return new ScamAnalysisResult(verdict, reason, score)
            {
                Match = bestMatch,
                Context = context
            };
        }

        private static int HammingDistance(long x, long y)
        {
            long val = x ^ y;
            int dist = 0;
            while (val != 0)
            {
                dist++;
                val &= val - 1;
            }
            return dist;
        }

        private static float CalculateScore(int aDiff, int dDiff, int pDiff)
        {
            // Normalize distances into a confidence score (0–1)
            float maxDist = 64f; 
            float weighted = (pDiff * 3f + dDiff * 2f + aDiff) / (3f + 2f + 1f);
            return 1f - Math.Clamp(weighted / maxDist, 0f, 1f);
        }
    }

}
