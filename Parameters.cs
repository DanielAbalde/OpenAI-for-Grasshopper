using Grasshopper.Kernel.Types;
using Grasshopper.Kernel;
using OpenAI.GPT3.Managers;
using OpenAI.GPT3.ObjectModels.ResponseModels; 

namespace OpenAI_for_Grasshopper
{
    public class Goo_OpenAIService : Goo_Base<OpenAIService>
    {
        public Goo_OpenAIService() : base() { }
        public Goo_OpenAIService(OpenAIService value) : base(value) { }
        public Goo_OpenAIService(Goo_OpenAIService other) : this(other.Value) { }

        public override IGH_Goo Duplicate()
        {
            return new Goo_OpenAIService(this);
        }

    }

    public class Param_OpenAIService : GH_Param<Goo_OpenAIService>
    {
        public override Guid ComponentGuid => new Guid("57a1127f-91ce-4ea4-82a2-21b871fd268a");
        public override GH_Exposure Exposure => GH_Exposure.hidden;

        public Param_OpenAIService() : base("Open AI Service", "OS", "The Open AI client service", "Extra", "OpenAI", GH_ParamAccess.item) { }
    }

    public class Goo_BaseResponse : Goo_Base<BaseResponse>
    {
        public Goo_BaseResponse() : base() { }
        public Goo_BaseResponse(BaseResponse value) : base(value) { }
        public Goo_BaseResponse(Goo_BaseResponse other) : this(other.Value) { }

        public override IGH_Goo Duplicate()
        {
            return new Goo_BaseResponse(this);
        }
    }

    public class Param_BaseResponse : GH_Param<Goo_BaseResponse>
    {
        public override Guid ComponentGuid => new Guid("4a0b09e3-81e3-47e1-872c-51cdc0fd4913");
        public override GH_Exposure Exposure => GH_Exposure.hidden;

        public Param_BaseResponse() : base("Open AI Response", "R", "The resulting response", "Extra", "OpenAI", GH_ParamAccess.item) { }
    }
}
