using UnityEngine;

namespace WanderingSouls
{
    public class SurvivorBrainComponent : MonoBehaviour
    {
        private EntityWanderingSurvivor survivor;
        private float tickTimer = 0f;
        private const float TICK_RATE = 2.0f;

        void Start()
        {
            survivor = GetComponent<EntityWanderingSurvivor>();
        }

        void Update()
        {
            if (survivor == null || survivor.IsDead()) return;

            tickTimer += Time.deltaTime;
            if (tickTimer >= TICK_RATE)
            {
                tickTimer = 0f;
                PerformLogic();
            }
        }

        private void PerformLogic()
        {
            survivor.factionId = (survivor.Loyalty < 10f) ? (byte)2 : (byte)1;

            if (survivor.State.Hunger > 70f)
            {
                // Food-seeking logic placeholder
            }

            if (survivor.GetAttackTarget() != null)
            {
                survivor.Attack(true);
            }
        }
    }
}
