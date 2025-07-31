using BrunoMikoski.AnimationSequencer;

namespace SeroJob.UiSystem
{
    public static class AnimationSequencerExtensions
    {
        public static float GetMaxDuration(this AnimationSequencerController animationSequencer)
        {
            if (animationSequencer == null || animationSequencer.AnimationSteps == null) return 0f;

            var result = 0f;

            foreach (var step in animationSequencer.AnimationSteps)
            {
                if (step == null) continue;
                if (step is GameObjectAnimationStep goStep)
                {
                    if (goStep.Duration > result) result = goStep.Duration;
                }
            }

            return result;
        }
    }
}