using Agent.Structures;
using System.Globalization;
using System.Text;

namespace Agent
{
    internal static class Program
    {
        public class EncodingStringWriter : StringWriter
        {
            private readonly Encoding _encoding;

            public EncodingStringWriter(Encoding encoding) : base()
            {
                _encoding = encoding;
            }

            public override Encoding Encoding
            {
                get { return _encoding; }
            }
        }

        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            DumpAnimations("e:/peedy.acs");
            DumpAnimations("e:/merlin.acs");
            DumpAnimations("e:/merlinsfx.acs");
            Console.ReadLine();
        }

        public static void DumpAnimations(string path)
        {
            AgentFile af = new(path);

            string[] anims = af.GetAnimationsNames();
            EncodingStringWriter sw = new(Encoding.ASCII);
            Console.WriteLine(sw.Encoding.EncodingName);

            string separator = CultureInfo.CurrentCulture.TextInfo.ListSeparator;

            foreach (string anim in anims)
            {
                AnimationInfo ai = af.ReadAnimation(anim);

                if (ai.Frames == null)
                    continue;

                sw.WriteLine($"Nome{separator}Tipo tran{separator}Anim ret.");
                sw.WriteLine($"{anim}{separator}{ai.TransitionType}{separator}{ai.ReturnAnimation}");
                sw.WriteLine($"Idx{separator}Dur{separator}Saida{separator}Imagens{separator}Leva a{separator}Qtd Overlays{separator}Idx audio");

                for (int i = 0; i < ai.Frames.Length; i++)
                {
                    FrameInfo fi = ai.Frames[i];
                    sw.WriteLine($"{i}{separator}{fi.Duration}{separator}{fi.ExitFrameIndex}{separator}{string.Join(", ", fi.Layers.Select(x => x.ImageIndex))}{separator}{string.Join(", ", fi.Branches.Select(x => x.TargetFrameIndex.ToString()))}{separator}{fi.MouthOverlays.Length}{separator}{fi.AudioIndex}");
                }
            }

            File.WriteAllText($"d:/desktop/{Path.GetFileNameWithoutExtension(path)}.csv", sw.ToString());

            Console.WriteLine("done");
        }
    }
}