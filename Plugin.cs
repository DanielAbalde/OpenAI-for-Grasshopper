using Grasshopper.Kernel;
using System.Drawing;

namespace OpenAI_for_Grasshopper
{
    public class Plugin : GH_AssemblyInfo
    {
        public override string Name => "OpenAI for Grasshopper";
        public override string Description => "OpenAI API interface for Grasshopper, using https://github.com/betalgo/openai";
        public override Bitmap Icon => Properties.Resources.OpenAI_logo_24x24;
        public override string AuthorName => "Daniel Gonzalez Abalde";
        public override string AuthorContact => "DaniGA#9856";
        public override string Version => "1.0.0";
        public override Guid Id => new Guid("0f8cbae1-bf53-4163-a828-edf4d16a1113");
        public override GH_LibraryLicense License => GH_LibraryLicense.opensource;
    }
}
