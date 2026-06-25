namespace WanderingSouls
{
    public class BrainState
    {
        public float Hunger, Thirst, Energy, Curiosity, Resources;
    }

    public enum SurvivorGoal
    {
        Idle,
        FindFood,
        FindWater,
        Rest,
        FollowPlayer,
        Building,
        Crafting,
        Scavenge
    }

    public class SurvivorBrain
    {
        private EntityWanderingSurvivor owner;
        public BrainState State => owner.State;
        public SurvivorGoal CurrentGoal { get; set; }

        public SurvivorBrain(EntityWanderingSurvivor survivor)
        {
            this.owner = survivor;
        }

        public void Think()
        {
            this.CurrentGoal = SurvivorDecision.DecideGoal(this);
        }
    }

    public static class SurvivorDecision
    {
        public static SurvivorGoal DecideGoal(SurvivorBrain brain)
        {
            if (brain.State.Hunger > 80f) return SurvivorGoal.FindFood;
            if (brain.State.Energy < 20f) return SurvivorGoal.Rest;
            return SurvivorGoal.FollowPlayer;
        }
    }
}
