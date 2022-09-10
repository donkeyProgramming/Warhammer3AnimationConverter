using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using Warhammer3AnimationConverter.Helpers;

namespace Warhammer3AnimationConverter
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Welcome to the simple Warhammer 2 => Warhammer 3 Animation fragment converter.");
            Console.WriteLine("\nHow to use:");
            Console.WriteLine("\t1. Grab the animation fragment file using AssetEditor and save it to a text file");
            Console.WriteLine("\t2. Drag the file onto the application");
            Console.WriteLine("\t3. Copy paste the content of the outputfile into AssetEditors Warhammer 3 animation Bin");
            Console.WriteLine();

            if (args.Length == 0)
            {
                Console.WriteLine("Error: No file found. Please drag a file onto the exe.");
                CloseApplication();
                return;
            }

            try
            {
                var outputFileName = "OutputFileName";
                var warhammer3Slots = new BaseAnimationSlotHelper(GameTypeEnum.Warhammer3);

                var inputFilePath = args.First();
                var fileContent = File.ReadAllText(inputFilePath);
                var warhammer2Animation = LoadWarhammer2Animation(fileContent, out var errorMessage);
                if (warhammer2Animation == null)
                {
                    Console.WriteLine("Error loading the warhammer 2 fragment");
                    Console.WriteLine($"XML error Message : {errorMessage.Text} at {errorMessage.ErrorLineNumber}");
                    CloseApplication();
                    return;
                }

                var outputAnimation = new Helpers.Warhammer3.XmlFormat();
                outputAnimation.Version = "Wh3";
                outputAnimation.Data = new Helpers.Warhammer3.GeneralBinData()
                {
                    TableVersion = 4,
                    TableSubVersion = 3,
                    Name = outputFileName,
                    SkeletonName = warhammer2Animation.Skeleton,
                    LocomotionGraph = "animations/locomotion_graphs/entity_locomotion_graph.xml",
                    UnknownValue1_RelatedToFlight = 0
                };
                var animationFragmentEntryGroupBySlots = warhammer2Animation.AnimationFragmentEntry.GroupBy(x => x.Slot).ToList();

                foreach (var animationFragmentEntryGroup in animationFragmentEntryGroupBySlots)
                {
                    var firstInstance = animationFragmentEntryGroup.First();

                    var animSlotName = firstInstance.Slot;
                    var warhammer3Slot = warhammer3Slots.GetfromValue(animSlotName);
                    if (warhammer3Slot == null)
                    {
                        animSlotName = animSlotName + " {== AnimationSlot not used by Warhammer3";
                    }

                    var warhammer3AnimationSlot = new Helpers.Warhammer3.Animation()
                    {
                        BlendId = firstInstance.BlendInTime.Value,
                        Slot = animSlotName,
                        BlendOut = firstInstance.SelectionWeight.Value,
                        Ref = new List<Helpers.Warhammer3.Instance>(),
                        WeaponBone = firstInstance.WeaponBone,
                    };

                    foreach (var instances in animationFragmentEntryGroup)
                    {
                        warhammer3AnimationSlot.Ref.Add(new Helpers.Warhammer3.Instance()
                        {
                            File = instances.File.Value,
                            Meta = instances.Meta.Value,
                            Sound = instances.Sound.Value
                        });
                    }

                    outputAnimation.Animations.Add(warhammer3AnimationSlot);
                }


                var warhammer3Text = GetText(outputAnimation);

                var outputDirectory = Path.GetDirectoryName(inputFilePath);
                var outfileName = Path.GetFileNameWithoutExtension(inputFilePath);
                var outputFinalePath = $"{outputDirectory}\\{outfileName}_wh3.txt";
                File.WriteAllText(outputFinalePath, warhammer3Text);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Something went wrong while converting - {e.Message}");
                Console.WriteLine(e.ToString());
            }

            CloseApplication();
        }


        static void CloseApplication()
        {
            Console.WriteLine("\n\nPress any key to close...");
            Console.ReadLine();
        }

        static string GetText(Helpers.Warhammer3.XmlFormat warhammer3Bin)
        {
            var xmlFrg = warhammer3Bin;

            var xmlserializer = new XmlSerializer(typeof(Helpers.Warhammer3.XmlFormat));
            var stringWriter = new StringWriter();
            XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
            ns.Add("", "");
            using (var writer = XmlWriter.Create(stringWriter, new XmlWriterSettings() { Indent = true, OmitXmlDeclaration = true }))
            {
                xmlserializer.Serialize(writer, xmlFrg, ns);
                var str = stringWriter.ToString();
                str = CleanUpXml(str);
                return str;
            }
        }

        static string CleanUpXml(string xmlText)
        {
            xmlText = xmlText.Replace("</BinEntry>", "</BinEntry>\n");
            xmlText = xmlText.Replace("<Bin>", "<Bin>\n");
            xmlText = xmlText.Replace("</GeneralBinData>", "</GeneralBinData>\n");
            return xmlText;
        }

        static Helpers.Warhammer2.Animation LoadWarhammer2Animation(string text, out ITextConverter.SaveError error)
        {
            var xmlserializer = new XmlSerializer(typeof(Helpers.Warhammer2.Animation));
            using var sr = new StringReader(text);
            using var reader = XmlReader.Create(sr);

            try
            {
                var errorHandler = new XmlSerializationErrorHandler();
                var obj = xmlserializer.Deserialize(reader, errorHandler.EventHandler) as Helpers.Warhammer2.Animation;

                if (errorHandler.Error != null)
                {
                    error = errorHandler.Error;
                    return null;
                }

                error =  null;
                return obj;
            }
            catch (Exception e)
            {
                var inner = GetInnerMostException(e);
                if (inner is XmlException xmlException)
                    error = new ITextConverter.SaveError() { Text = xmlException.Message, ErrorLineNumber = xmlException.LineNumber, ErrorPosition = xmlException.LinePosition, ErrorLength = 0 };
                else if (inner != null)
                    error = new ITextConverter.SaveError() { Text = e.Message + " - " + inner.Message, ErrorLineNumber = 1 };
                else
                    error = new ITextConverter.SaveError() { Text = e.Message, ErrorLineNumber = 1 };
                return null;
            }
        }

        public static Exception GetInnerMostException(Exception e)
        {
            var innerE = e.InnerException;
            if (innerE == null)
                return e;

            while (innerE.InnerException != null)
            {
                innerE = innerE.InnerException;
            }

            return innerE;
        }
    }
}
