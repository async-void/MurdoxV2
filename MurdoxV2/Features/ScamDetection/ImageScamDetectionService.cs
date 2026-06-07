using MurdoxV2.Enums;
using MurdoxV2.Hashing;
using SkiaSharp;
using System.Drawing;

namespace MurdoxV2.Features.ScamDetection
{
    public class ImageScamDetectionService(IScamHashRepository repo)
    {
        private readonly IScamHashRepository _repo = repo;

        // Thresholds for determining scam severity
        private const int AHASH_THRESHOLD = 10;
        private const int DHASH_THRESHOLD = 10;
        private const int PHASH_THRESHOLD = 15;

        public async Task<ScamAnalysisResult> AnalyzeAsync(SKBitmap bitmap)
        {
            // 1. Compute perceptual hashes
            long aHash = ImageHashing.AverageHash(bitmap);
            long dHash = ImageHashing.DifferenceHash(bitmap);
            long pHash = ImageHashing.PerceptualHash(bitmap);

            // 2. Load known scam hashes
            var known = await _repo.GetAllAsync();

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

            if (bestMatch == null)
                return new ScamAnalysisResult(ScamVerdict.Clean, "No match found", 0f);

            // 3. Compute distances for the best match
            int aDiff = HammingDistance(aHash, bestMatch.AHash);
            int dDiff = HammingDistance(dHash, bestMatch.DHash);
            int pDiff = HammingDistance(pHash, bestMatch.PHash);

            // 4. Determine verdict
            ScamVerdict verdict;
            string reason;

            if (pDiff <= PHASH_THRESHOLD && dDiff <= DHASH_THRESHOLD && aDiff <= AHASH_THRESHOLD)
            {
                verdict = ScamVerdict.Scam;
                reason = "Matched known scam image";
            }
            else if (pDiff <= PHASH_THRESHOLD + 5)
            {
                verdict = ScamVerdict.Suspicious;
                reason = "Image is visually similar to known scam templates";
            }
            else
            {
                verdict = ScamVerdict.Clean;
                reason = "No significant similarity to known scam images";
            }

            // 5. Convert distances into a 0–1 score
            float score = CalculateScore(aDiff, dDiff, pDiff);

            return new ScamAnalysisResult(verdict, reason, score);
        }

        private static int HammingDistance(long x, long y)
        {
            long v = x ^ y;
            int dist = 0;

            while (v != 0)
            {
                dist++;
                v &= v - 1;
            }

            return dist;
        }

        private static float CalculateScore(int a, int d, int p)
        {
            // Normalize distances (0 = identical, 1 = completely different)
            float aNorm = a / 64f;
            float dNorm = d / 64f;
            float pNorm = p / 64f;

            // Weighted similarity score (1 = identical, 0 = unrelated)
            float similarity =
                (1f - aNorm) * 0.2f +
                (1f - dNorm) * 0.3f +
                (1f - pNorm) * 0.5f;

            return similarity;
        }
    }

}
