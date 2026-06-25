using System.IO;
using System.Collections.Generic;

namespace WanderingSouls
{
    public class EntityWanderingSurvivor : EntityNPC
    {
        public PersonalityTraits Traits = new PersonalityTraits();
        public string SurvivorName = "Unknown";
        public BrainState State = new BrainState();
        public float Loyalty = 50f;

        public override void PostInit()
        {
            base.PostInit();

            if (SurvivorName == "Unknown")
            {
                Traits = PersonalityTraits.GenerateRandom();
                SurvivorName = NameGenerator.GenerateName("male", Traits);
            }

            this.factionId = 1;
        }

        public override void Write(BinaryWriter _bw, bool _bNetworkWrite)
        {
            base.Write(_bw, _bNetworkWrite);

            _bw.Write(SurvivorName);
            _bw.Write(Loyalty);
            _bw.Write(State.Hunger);
            _bw.Write(State.Thirst);
            _bw.Write(State.Energy);

            _bw.Write(Traits.Traits.Count);
            foreach (var trait in Traits.Traits)
            {
                _bw.Write((int)trait);
            }
        }

        public override void Read(byte _version, BinaryReader _br)
        {
            base.Read(_version, _br);

            SurvivorName = _br.ReadString();
            Loyalty = _br.ReadSingle();
            State.Hunger = _br.ReadSingle();
            State.Thirst = _br.ReadSingle();
            State.Energy = _br.ReadSingle();

            int count = _br.ReadInt32();
            Traits.Traits.Clear();
            for (int i = 0; i < count; i++)
            {
                Traits.Traits.Add((Trait)_br.ReadInt32());
            }
        }
    }
}
