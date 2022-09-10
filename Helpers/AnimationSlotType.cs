using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Warhammer3AnimationConverter.Helpers
{
    public enum GameTypeEnum
    {
        Unknown = -1,
        Arena = 0,
        Attila,
        Empire,
        Napoleon,
        Rome_2_Remastered,
        Rome_2,
        Shogun_2,
        ThreeKingdoms,
        ThronesOfBritannia,
        Warhammer1,
        Warhammer2,
        Warhammer3,
        Troy
    }

    [Serializable]
    public class AnimationSlotType
    {
        public int Id { get; set; }
        public string Value { get; set; }

        public AnimationSlotType(int id, string value)
        {
            Id = id;
            Value = value.ToUpper();
        }

        public AnimationSlotType()
        { }

        public AnimationSlotType Clone()
        {
            return new AnimationSlotType(Id, Value);
        }

        public override string ToString()
        {
            return $"{Value}[{Id}]";
        }
    }

    public class BaseAnimationSlotHelper
    {
        public List<AnimationSlotType> Values { get; private set; }

        public BaseAnimationSlotHelper(GameTypeEnum game)
        {
            switch (game)
            {
                case GameTypeEnum.Warhammer2:
                    Load("Warhammer3AnimationConverter.Resources.Warhammer2AnimationSlots.txt");
                    break;

                case GameTypeEnum.Warhammer3:
                    Load("Warhammer3AnimationConverter.Resources.Warhammer3AnimationSlots.txt");
                    break;

                default:
                    throw new Exception();

            }
        }

        public AnimationSlotType GetFromId(int id)
        {
            return Values[id];
        }

        public AnimationSlotType GetfromValue(string value)
        {
            var upperStr = value.ToUpper();
            return Values.FirstOrDefault(x => x.Value == upperStr);
        }

        public AnimationSlotType GetMatchingRiderAnimation(string value)
        {
            var riderAnim = "RIDER_" + value;
            return Values.FirstOrDefault(x => x.Value == riderAnim);
        }


        void Load(string resourcePath)
        {
            Values = new List<AnimationSlotType>();
            using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourcePath);
            using (var reader = new StreamReader(stream))
            {
                string[] result = reader.ReadToEnd().Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                for(int i = 0; i < result.Length; i++)
                    Values.Add(new AnimationSlotType(i, result[i].Trim()));
            }
        }
    }

    public static class AnimationSlotTypeHelperWh3
    {
        static BaseAnimationSlotHelper Instance;
        public static List<AnimationSlotType> Values { get { Create(); return Instance.Values; } }

        static public AnimationSlotType GetFromId(int id)
        {
            Create();
            return Values[id];
        }

        static public AnimationSlotType GetfromValue(string value)
        {
            Create();
            var upperStr = value.ToUpper();
            return Values.FirstOrDefault(x => x.Value == upperStr);
        }

        static public AnimationSlotType GetMatchingRiderAnimation(string value)
        {
            var riderAnim = "RIDER_" + value;
            return Values.FirstOrDefault(x => x.Value == riderAnim);
        }

        static void Create()
        {
            if (Instance == null)
                Instance = new BaseAnimationSlotHelper(GameTypeEnum.Warhammer3);
        }

        public static BaseAnimationSlotHelper GetInstance()
        {
            Create();
            return Instance;
        }
    }

    public static class DefaultAnimationSlotTypeHelper
    {
        static BaseAnimationSlotHelper Instance;
        public static List<AnimationSlotType> Values { get { Create(); return Instance.Values; } }

        static public AnimationSlotType GetFromId(int id)
        {
            Create();
            return Values[id];
        }

        static public AnimationSlotType GetfromValue(string value)
        {
            Create();
            var upperStr = value.ToUpper();
            return Values.FirstOrDefault(x => x.Value == upperStr);
        }

        static public AnimationSlotType GetMatchingRiderAnimation(string value)
        {
            var riderAnim = "RIDER_" + value;
            return Values.FirstOrDefault(x => x.Value == riderAnim);
        }

        static void Create()
        {
            if(Instance == null)
                Instance = new BaseAnimationSlotHelper(GameTypeEnum.Warhammer2);
        }

        public static BaseAnimationSlotHelper GetInstance()
        {
            Create();
            return Instance;
        }
    }
}

