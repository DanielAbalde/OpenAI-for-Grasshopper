using Grasshopper.Kernel.Types;
using Grasshopper.Kernel;
using OpenAI.GPT3.Managers;
using OpenAI.GPT3.ObjectModels.ResponseModels;
using System.Drawing;

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
        protected override Bitmap Icon => Properties.Resources.OpenAI_logo_24x24;

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
        protected override Bitmap Icon => Properties.Resources.OpenAI_logo_24x24;

        public Param_BaseResponse() : base("Open AI Response", "R", "The resulting response", "Extra", "OpenAI", GH_ParamAccess.item) { }
    }

    public class Goo_ChatMessage : GH_Goo<OpenAI.GPT3.ObjectModels.RequestModels.ChatMessage>
    {
        public override bool IsValid => m_value != null; 
        public override string TypeName => "ChatMessage"; 
        public override string TypeDescription => "ChatGPT message";

        public Goo_ChatMessage() : base() { }
        public Goo_ChatMessage(OpenAI.GPT3.ObjectModels.RequestModels.ChatMessage value) : base(value) { }
        public Goo_ChatMessage(Goo_ChatMessage other) : this(other.Value) { }

        public override IGH_Goo Duplicate()
        {
            return new Goo_ChatMessage(this);
        }

        public override string ToString()
        {
            if(m_value != null) {
                return $"({m_value.Role}) {m_value.Content}";
            } else {
                return "Null " + TypeName;
            }
        }

        public override bool CastFrom(object source)
        {
            if(source is OpenAI.GPT3.ObjectModels.RequestModels.ChatMessage cm) {
                m_value = cm;
                return true;
            }
            if(source is Goo_ChatMessage gcm) {
                m_value = gcm.Value;
                return true;
            }
            if(source is IGH_Goo goo) {
                return CastFrom(goo.ScriptVariable());
            }
            if(source is string s) {
                m_value = OpenAI.GPT3.ObjectModels.RequestModels.ChatMessage.FromUser(s);
                return true;
            }
            return base.CastFrom(source);
        }
        public override bool CastTo<Q>(ref Q target)
        {
            var typeOfQ = typeof(Q);
            if(typeOfQ == typeof(OpenAI.GPT3.ObjectModels.RequestModels.ChatMessage)) {
                if (m_value != null) {
                    target = (Q)(object)m_value;
                }
                return true;
            }
            if (typeOfQ == typeof(string)) {
                if (m_value != null) {
                    target = (Q)(object)m_value.Content;
                }
                return true;
            } 
            return base.CastTo(ref target);
        }
    }

    public class Param_ChatMessage : GH_PersistentParam<Goo_ChatMessage>
    {
        public override Guid ComponentGuid => new Guid("297cfb51-5e41-48b6-a7fc-3c3f9c614481");
        public override GH_Exposure Exposure => GH_Exposure.hidden;
        protected override Bitmap Icon => Properties.Resources.OpenAI_logo_24x24;

        public Param_ChatMessage() : base(new GH_InstanceDescription("ChatGPT Message", "M", "A role-based message for ChatGPT", "Extra", "OpenAI")) { }

        protected override GH_GetterResult Prompt_Singular(ref Goo_ChatMessage value)
        {
            return GH_GetterResult.cancel;
        }

        protected override GH_GetterResult Prompt_Plural(ref List<Goo_ChatMessage> values)
        {
            return GH_GetterResult.cancel;
        }
    }

}
