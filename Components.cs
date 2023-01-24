using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using OpenAI.GPT3.Managers;
using OpenAI.GPT3.ObjectModels.ResponseModels;
using OpenAI.GPT3.ObjectModels.ResponseModels.ModelResponseModels;
using OpenAI.GPT3.ObjectModels.SharedModels;
using OpenAI.GPT3.ObjectModels.ResponseModels.ImageResponseModel;
using OpenAI.GPT3.ObjectModels.ResponseModels.FineTuneResponseModels;
using OpenAI.GPT3.ObjectModels.ResponseModels.FileResponseModels;
using System.Drawing;
using System.Net;

/*
360a36db-b8df-4559-bb9e-9118d512acad
c0ab69dd-5cf4-488c-bb11-0ed989c31dfe
b117ca1b-8f20-44be-9fa9-c6a3a3aa6b7e
2db7c739-51b5-4cad-b3d4-e0469375ca1d
035f2240-4e0e-4e06-ab73-e3a92c9012b9
db385d9b-24d7-4dbf-9972-4cf9977c3e01
72ae4691-f50a-4c23-91ac-4531a950b0c9
714c9774-a25b-4f91-b875-11bf2ff380a4
d5b98626-2bf4-44dd-98ad-ce909f8bde67
e70a8928-d012-4139-acdb-d5eda7c6d745
68a2235e-60a7-400e-b2ec-41b879e67b87
8eb13eb7-ef33-4c15-95bb-0e3cd226b456
44c90112-3f35-4916-949a-78cdacbc3e62
593e1596-b04a-4aec-91f9-3b7d7e8def99
79407223-ff89-4cf9-b075-1118b12ac6e3
64e40d78-5f29-4f6e-875a-1194155a57a7
b0e255f6-28a6-4ec0-8bac-9ae9f09c62a7
fb46c7c8-8ca2-4250-9cdb-d92d5e69a98b
0044a2b1-6c2d-4a93-b6a6-80de5932b5ea
ac4df529-d20f-4021-a18a-1f54a92353c5
297cfb51-5e41-48b6-a7fc-3c3f9c614481
54e9cd9a-381b-4cfb-8605-38042fe03f86
fba0d734-0e1a-4404-aaa5-8fc0b71a7122
62b04def-2904-4669-ae68-2bc9a75d09cc

*/

namespace OpenAI_for_Grasshopper
{
    #region Base
    public abstract class Goo_Base<T> : GH_Goo<T>
    {
        public override bool IsValid => m_value != null; 
        public override string TypeName => nameof(T); 
        public override string TypeDescription => TypeName;

        public Goo_Base() : base() { }
        public Goo_Base(T value) : base(value) { }
        public Goo_Base(Goo_Base<T> other) : this(other.Value) { }

        public override string ToString()
        {
            if(m_value == null) {
                return "Null" + TypeName;
            } else {
                return m_value.ToString();
            }
        }
         
        public override bool CastFrom(object source)
        {
            if(source == null)
            {
                return false;
            }
            if(source is T t)
            {
                m_value = t;
                return true;
            }
            else if(source is IGH_Goo goo)
            {
                return CastFrom(goo.ScriptVariable());
            }
            return base.CastFrom(source);
        }
        public override bool CastTo<Q>(ref Q target)
        {
            if(typeof(Q) == typeof(T))
            {
                if(m_value != null) {
                    target = (Q)(object)m_value;
                }
                return true;
            }
            return base.CastTo(ref target);
        }
    }

    public abstract class Comp_Base : GH_Component
    {
        protected override Bitmap Icon => Properties.Resources.OpenAI_logo_24x24;

        public Comp_Base(string name, string nickname, string description) : base(name, nickname, description, "Extra", "OpenAI") { }
    }

    public abstract class Comp_BaseResponse<T> : GH_TaskCapableComponent<T> where T : BaseResponse
    {
        public override GH_Exposure Exposure => GH_Exposure.secondary;
        protected override Bitmap Icon => Properties.Resources.OpenAI_logo_24x24;

        public Comp_BaseResponse(string name, string nickname, string description) : base(name, nickname, description, "Extra", "OpenAI") { }
        
        protected void AddAskParameter(GH_InputParamManager pManager, bool defaultValue = false)
        {
            pManager.AddBooleanParameter("Ask", "Ask", "Perform the request", GH_ParamAccess.item, defaultValue);
        }
        protected abstract Task<T> SolveInstanceInPreSolve(IGH_DataAccess DA, out bool ask);
        protected abstract void SolveInstanceInPostSolve(IGH_DataAccess DA, T result);
     
        private Dictionary<int, bool> _asks = new Dictionary<int, bool>();

        protected override void BeforeSolveInstance()
        {
            _asks.Clear();
        }
      
        protected override sealed void SolveInstance(IGH_DataAccess DA)
        { 
            if (InPreSolve) {
                Task<T> task = SolveInstanceInPreSolve(DA, out bool ask);
                _asks.Add(DA.Iteration, ask);
                if (task != null) {
                    TaskList.Add(task);
                }
                return;
            }

            if (_asks[DA.Iteration]) {
                Message = "Asking...";
                Grasshopper.Instances.RedrawCanvas();
                if (!GetSolveResults(DA, out T result)) {

                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Failed");
                    return;
                }
                Message = "Done";

                if (result != null) {
                    if(result.Error != null) {
                        AddRuntimeMessage(GH_RuntimeMessageLevel.Error, result.Error.Message);
                    } else {
                        SolveInstanceInPostSolve(DA, result);
                    } 
                }

            } else {
                Message = "Ready";
            } 
        }
    }
    #endregion
     
    #region Expo1
    public class Comp_OpenAIService : Comp_Base
    {
        public override Guid ComponentGuid => new Guid("fa5eeb5e-fecf-44df-b3b6-9a1b13bea2d2");
        public override GH_Exposure Exposure => GH_Exposure.primary;

        public Comp_OpenAIService() : base("Open AI Service", "Open AI", "Open AI client service") { }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("API Key", "API", "The OpenAI API uses API keys for authentication. Visit your API Keys page to\r\n        //     retrieve the API key you'll use in your requests. Remember that your API key\r\n        //     is a secret! Do not share it with others or expose it in any client-side code(browsers,\r\n        //     apps). Production requests must be routed through your own backend server where\r\n        //     your API key can be securely loaded from an environment variable or key management\r\n        //     service.", GH_ParamAccess.item);
            pManager.AddTextParameter("Organization Id", "Org", "For users who belong to multiple organizations, you can pass a header to specify\r\n        //     which organization is used for an API request. Usage from these API requests\r\n        //     will count against the specified organization's subscription quota. Organization\r\n        //     IDs can be found on your Organization settings page.", GH_ParamAccess.item);
            Params.Input[^1].Optional = true;
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddParameter(new Param_OpenAIService());
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string api = string.Empty;

            if (!DA.GetData(0, ref api))
                return;

            if (string.IsNullOrEmpty(api)) {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "No API key");
                return;
            }

            var options = new OpenAI.GPT3.OpenAiOptions() { ApiKey = api };
            var org = string.Empty;
            if(DA.GetData(1, ref org) && !string.IsNullOrEmpty(org)) {
                options.Organization = org; 
            }
            
            var service = new OpenAI.GPT3.Managers.OpenAIService(options);

            DA.SetData(0, service);
        }
    }

    public class Comp_OpenAIModels : Comp_BaseResponse<ModelListResponse>
    {
        public override Guid ComponentGuid => new Guid("a5ca4009-9df2-4afd-81a9-404dba2533eb");
        public override GH_Exposure Exposure => GH_Exposure.primary;

        public Comp_OpenAIModels() : base("Models", "Models", "List Open AI models") { }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddParameter(new Param_OpenAIService());
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Roots", "R", "The base model name", GH_ParamAccess.list);
            pManager.AddTextParameter("Owners", "O", "The owner of the model", GH_ParamAccess.list);
            pManager.AddTextParameter("Parent", "P", "The parent model", GH_ParamAccess.list);
            pManager.AddParameter(new Param_BaseResponse());
        }

        protected override Task<ModelListResponse> SolveInstanceInPreSolve(IGH_DataAccess DA, out bool ask)
        {
            OpenAIService service = null;
            ask = true;
            if (!DA.GetData(0, ref service)) {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Failed to service");
                return null;
            }

            return Task.Run(() => service.Models.ListModel());
        }

        protected override void SolveInstanceInPostSolve(IGH_DataAccess DA, ModelListResponse result)
        {
            DA.SetDataList(0, result.Models.Select(m => m.Root));
            DA.SetDataList(1, result.Models.Select(m => m.Owner));
            DA.SetDataList(2, result.Models.Select(m => m.Parent));
            DA.SetData(3, result);
        }
    }
    #endregion

    #region Expo2
    public class Comp_OpenAICompletion : Comp_BaseResponse<CompletionCreateResponse>
    {
        public override Guid ComponentGuid => new Guid("7f25a457-5d97-470e-9193-ae58dc503477");

        public Comp_OpenAICompletion() : base("Completion", "Completion", "Given a prompt, the model will return one or more predicted completions, and can also return the probabilities of alternative tokens at each position.") { }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddParameter(new Param_OpenAIService());
            pManager.AddTextParameter("Model", "Mod", "", GH_ParamAccess.item, "text-davinci-003");
            pManager.AddTextParameter("Prompts", "Prm", "", GH_ParamAccess.list);
            pManager.AddIntegerParameter("Token Limit", "Lim", "The maximum number of tokens to generate in the completion. The token count of\r\n        //     your prompt plus max_tokens cannot exceed the model's context length. Most models\r\n        //     have a context length of 2048 tokens (except davinci-codex, which supports 4096).", GH_ParamAccess.item, 200);
            pManager.AddNumberParameter("Temperature", "Tmp", "What sampling temperature to use. Higher values means the model will take more\r\n        //     risks. Try 0.9 for more creative applications, and 0 (argmax sampling) for ones\r\n        //     with a well-defined answer. We generally recommend altering this or top_p but\r\n        //     not both.", GH_ParamAccess.item, 0.5);
            pManager.AddNumberParameter("TopP", "ToP", "An alternative to sampling with temperature, called nucleus sampling, where the\r\n        //     model considers the results of the tokens with top_p probability mass. So 0.1\r\n        //     means only the tokens comprising the top 10% probability mass are considered.\r\n        //     We generally recommend altering this or temperature but not both.", GH_ParamAccess.item, 0.5);
            AddAskParameter(pManager);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Result", "R", "", GH_ParamAccess.list);
            pManager.AddIntegerParameter("Usage", "U", "Total tokens of usage", GH_ParamAccess.item);
            pManager.AddParameter(new Param_BaseResponse());
        }

        protected override Task<CompletionCreateResponse> SolveInstanceInPreSolve(IGH_DataAccess DA, out bool ask)
        {
            OpenAIService service = null;
            string model = null;
            List<string> prompts = new List<string>();
            int tokenLimit = 100;
            double temperature = 0.5;
            double topP = 0.5;
            ask = false;
           
            if (!DA.GetData(0, ref service) || !DA.GetData(1, ref model) || !DA.GetDataList(2, prompts) || !DA.GetData(3, ref tokenLimit) ||
                  !DA.GetData(4, ref temperature) || !DA.GetData(5, ref topP) || !DA.GetData(6, ref ask)) {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Failed to get some input");
                return null;
            }
           
            var request = new OpenAI.GPT3.ObjectModels.RequestModels.CompletionCreateRequest() {
                PromptAsList = prompts,
                Model = model, 
                MaxTokens = tokenLimit,
                Temperature = (float)Math.Max(0, Math.Min(1, temperature)),
                TopP = (float)Math.Max(0, Math.Min(1, topP)),
            };
            return Task.Run(() => service.CreateCompletion(request), CancelToken);

        }

        protected override void SolveInstanceInPostSolve(IGH_DataAccess DA, CompletionCreateResponse result)
        {
            DA.SetDataList(0, result.Choices.Select(c => c.Text));
            DA.SetData(1, result.Usage.TotalTokens);
            DA.SetData(2, result);
        }
    }

    public class Comp_OpenAIEmbeddings : Comp_BaseResponse<EmbeddingCreateResponse>
    {
        public override Guid ComponentGuid => new Guid("8f2fcdb1-f8a7-4f23-b252-37af1c44a547");

        public Comp_OpenAIEmbeddings() : base("Embedding", "Embedding", "Get a vector representation of a given input that can be easily consumed by machine learning models and algorithms.") { }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddParameter(new Param_OpenAIService());
            pManager.AddTextParameter("Model", "Mod", "ID of the model to use. You can use the [List models](/docs/api-reference/models/list)\r\n        //     API to see all of your available models, or see our [Model overview](/docs/models/overview)\r\n        //     for descriptions of them.", GH_ParamAccess.item, "text-embedding-ada-002");
            pManager.AddTextParameter("Inputs", "Inp", "Input text to get embeddings for, encoded as a string or array of tokens. To\r\n        //     get embeddings for multiple inputs in a single request, pass an array of strings\r\n        //     or array of token arrays. Each input must not exceed 2048 tokens in length. Unless\r\n        //     your are embedding code, we suggest replacing newlines (`\\n`) in your input with\r\n        //     a single space, as we have observed inferior results when newlines are present.", GH_ParamAccess.list);
            AddAskParameter(pManager);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Result", "R", "", GH_ParamAccess.list);
            pManager.AddIntegerParameter("Usage", "U", "Total tokens of usage", GH_ParamAccess.item);
            pManager.AddParameter(new Param_BaseResponse());
        }

        protected override Task<EmbeddingCreateResponse> SolveInstanceInPreSolve(IGH_DataAccess DA, out bool ask)
        {
            OpenAIService service = null;
            string model = null;
            List<string> prompts = new List<string>();
            ask = false;
            if (!DA.GetData(0, ref service) || !DA.GetData(1, ref model) || !DA.GetDataList(2, prompts) || !DA.GetData(3, ref ask)) {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Failed to get some input");
                return null;
            }
            var request = new OpenAI.GPT3.ObjectModels.RequestModels.EmbeddingCreateRequest() {
                InputAsList = prompts,
                Model = model,
            };
            return Task.Run(() => service.CreateEmbedding(request), CancelToken);
        }

        protected override void SolveInstanceInPostSolve(IGH_DataAccess DA, EmbeddingCreateResponse result)
        {
            var tree = new DataTree<double>();
            if(result.Data != null) {
                for (int i = 0; i < result.Data.Count; i++) {
                    var path = new GH_Path(DA.Iteration, i); 
                    tree.AddRange(result.Data[i].Embedding, path);
                }
            } 
            DA.SetDataTree(0, tree);
            DA.SetData(1, result?.Usage.TotalTokens);
            DA.SetData(2, result);
        }
    }

    public class Comp_OpenAIImage : Comp_BaseResponse<ImageCreateResponse>
    {
        public override Guid ComponentGuid => new Guid("770be8e7-7cf5-4a83-9175-2fd6064ab4d4");

        public Comp_OpenAIImage() : base("Image", "Image", "Given a prompt and/or an input image, the model will generate a new image.") { }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddParameter(new Param_OpenAIService()); 
            pManager.AddTextParameter("Prompt", "Prm", "A text description of the desired image(s). The maximum length is 1000 characters.", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Count", "Cnt", "The number of images to generate. Must be between 1 and 10.", GH_ParamAccess.item, 1);
            pManager.AddTextParameter("Size", "Sze", "The size of the generated images. Must be one of 256x256, 512x512, or 1024x1024.", GH_ParamAccess.item, "256x256");
            AddAskParameter(pManager);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Bitmaps", "B", "Resulting images", GH_ParamAccess.list);
            pManager.AddTextParameter("URLs", "U", "Resulting image URLs", GH_ParamAccess.list);
            pManager.AddParameter(new Param_BaseResponse());
        }

        protected override Task<ImageCreateResponse> SolveInstanceInPreSolve(IGH_DataAccess DA, out bool ask)
        {
            OpenAIService service = null; 
            var prompt = string.Empty;
            var count = 1;
            var size = string.Empty;
            ask = false;
            if (!DA.GetData(0, ref service) || !DA.GetData(1, ref prompt) || !DA.GetData(2, ref count) || !DA.GetData(3, ref size) || !DA.GetData(4, ref ask)) {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Failed to get some input");
                return null;
            }

            if(size != "256x256" && size != "512x512" && size != "1024x1024") {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid size, please use 256x256 or 512x512 or 1024x1024");
                return null;
            }
            var request = new OpenAI.GPT3.ObjectModels.RequestModels.ImageCreateRequest() {
                Prompt = prompt,
                N = count,
                Size = size
            };
            return Task.Run(() => service.CreateImage(request), CancelToken);
        }

        protected override void SolveInstanceInPostSolve(IGH_DataAccess DA, ImageCreateResponse result)
        {
            var imgs = new List<GH_ObjectWrapper>();
            var urls = new List<string>();
            foreach(var r in result.Results) {
                urls.Add(r.Url);
                if (!string.IsNullOrEmpty(r.B64)) {
                    imgs.Add(new GH_ObjectWrapper(Utils.Base64ToImage(r.B64)));
                } else {
                    using (WebClient webClient = new WebClient()) {
                        byte[] data = webClient.DownloadData(r.Url);

                        using (MemoryStream mem = new MemoryStream(data)) {
                            using (var img = Image.FromStream(mem)) {
                                var tempPath = Path.GetTempPath() + Guid.NewGuid() + ".png";
                                img.Save(tempPath);
                                imgs.Add(new GH_ObjectWrapper(img.Clone()));
                                File.Delete(tempPath);
                            }
                        }

                    }
                }
            }
            DA.SetDataList(0, imgs);
            DA.SetDataList(1, urls); 
            DA.SetData(2, result);
        }
         
    }
     
    public class Comp_OpenAIImageVariation : Comp_BaseResponse<ImageCreateResponse>
    {
        public override Guid ComponentGuid => new Guid("803e728d-ea2c-46a3-8c72-d3ec892f4483");

        public Comp_OpenAIImageVariation() : base("Image Variation", "Image Variation", "Creates a variation of a given image.") { }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddParameter(new Param_OpenAIService());
            pManager.AddGenericParameter("Image", "Img", "The image to edit. Must be a valid PNG file, less than 4MB, and square.", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Count", "Cnt", "The number of images to generate. Must be between 1 and 10.", GH_ParamAccess.item, 1);
            pManager.AddTextParameter("Size", "Sze", "The size of the generated images. Must be one of 256x256, 512x512, or 1024x1024.", GH_ParamAccess.item, "256x256");
            AddAskParameter(pManager);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Bitmaps", "B", "Resulting images", GH_ParamAccess.list);
            pManager.AddTextParameter("URLs", "U", "Resulting image URLs", GH_ParamAccess.list);
            pManager.AddParameter(new Param_BaseResponse());
        }

        protected override Task<ImageCreateResponse> SolveInstanceInPreSolve(IGH_DataAccess DA, out bool ask)
        {
            OpenAIService service = null;
            GH_ObjectWrapper imageGoo = null;
            var count = 1;
            var size = string.Empty;
            ask = false;
            if (!DA.GetData(0, ref service) || !DA.GetData(1, ref imageGoo) || !DA.GetData(2, ref count) || !DA.GetData(3, ref size) || !DA.GetData(4, ref ask)) {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Failed to get some input");
                return null;
            }

            if (size != "256x256" && size != "512x512" && size != "1024x1024") {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid size, please use 256x256 or 512x512 or 1024x1024");
                return null;
            }

            var image = imageGoo.Value;

            string b64img = string.Empty;
            if(image is string imageString) {
                if (File.Exists(imageString)) {
                    var img = Image.FromFile(imageString);
                    if (img == null) {
                        AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Failed to load image");
                        return null;
                    }
                    b64img = Utils.ImageToBase64(img, System.Drawing.Imaging.ImageFormat.Png);
                } else {
                    b64img = imageString; 
                }
            } else if (image is Bitmap bmp) {
                b64img = Utils.ImageToBase64(bmp, System.Drawing.Imaging.ImageFormat.Png);
            } else if (image is Image img) {
                b64img = Utils.ImageToBase64(img, System.Drawing.Imaging.ImageFormat.Png);
            }
            if (string.IsNullOrEmpty(b64img)) {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Failed to get image");
                return null;
            }

            var bytes = Convert.FromBase64String(b64img);
            
            var request = new OpenAI.GPT3.ObjectModels.RequestModels.ImageVariationCreateRequest() {
                Image = bytes,
                N = count,
                Size = size, ImageName = "UserImage"
            };
            return Task.Run(() => service.CreateImageVariation(request), CancelToken);
        }

        protected override void SolveInstanceInPostSolve(IGH_DataAccess DA, ImageCreateResponse result)
        {
            var imgs = new List<GH_ObjectWrapper>();
            var urls = new List<string>();
            foreach (var r in result.Results) {
                urls.Add(r.Url);
                if (!string.IsNullOrEmpty(r.B64)) {
                    imgs.Add(new GH_ObjectWrapper(Utils.Base64ToImage(r.B64)));
                } else {
                    using (WebClient webClient = new WebClient()) {
                        byte[] data = webClient.DownloadData(r.Url);

                        using (MemoryStream mem = new MemoryStream(data)) {
                            using (var img = Image.FromStream(mem)) {
                                var tempPath = Path.GetTempPath() + Guid.NewGuid() + ".png";
                                img.Save(tempPath);
                                imgs.Add(new GH_ObjectWrapper(img.Clone()));
                                File.Delete(tempPath);
                            }
                        }

                    }
                }
            }
            DA.SetDataList(0, imgs);
            DA.SetDataList(1, urls);
            DA.SetData(2, result);
        }

    }

    public class Comp_OpenAIImageEdit: Comp_BaseResponse<ImageCreateResponse>
    {
        public override Guid ComponentGuid => new Guid("a51f075a-6729-43e2-8b12-5a8bd3aabbdf");

        public Comp_OpenAIImageEdit() : base("Image Edit", "Image Edit", "Creates an edited or extended image given an original image and a prompt.") { }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddParameter(new Param_OpenAIService());
            pManager.AddGenericParameter("Image", "Img", "Bitmap or file path or base64 image. Must be png, less than 4MB and square", GH_ParamAccess.item);
            pManager.AddGenericParameter("Mask", "Msk", "An additional image whose fully transparent areas (e.g. where alpha is zero)\r\n        //     indicate where image should be edited. Must be a valid PNG file, less than 4MB,\r\n        //     and have the same dimensions as image.", GH_ParamAccess.item);
            pManager.AddTextParameter("Prompt", "Prm", "A text description of the desired image(s). The maximum length is 1000 characters.", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Count", "Cnt", "The number of images to generate. Must be between 1 and 10.", GH_ParamAccess.item, 1);
            pManager.AddTextParameter("Size", "Sze", "The size of the generated images. Must be one of 256x256, 512x512, or 1024x1024.", GH_ParamAccess.item, "256x256");
            AddAskParameter(pManager);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Bitmaps", "B", "Resulting images", GH_ParamAccess.list);
            pManager.AddTextParameter("URLs", "U", "Resulting image URLs", GH_ParamAccess.list);
            pManager.AddParameter(new Param_BaseResponse());
        }

        protected override Task<ImageCreateResponse> SolveInstanceInPreSolve(IGH_DataAccess DA, out bool ask)
        {
            OpenAIService service = null;
            GH_ObjectWrapper imageGoo = null;
            GH_ObjectWrapper maskGoo = null;
            var prompt = string.Empty;
            var count = 1;
            var size = string.Empty;
            ask = false;
            if (!DA.GetData(0, ref service) || !DA.GetData(1, ref imageGoo) || !DA.GetData(2, ref maskGoo) ||
                !DA.GetData(3, ref prompt) || !DA.GetData(4, ref count) || !DA.GetData(5, ref size) || !DA.GetData(6, ref ask)) {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Failed to get some input");
                return null;
            }

            if (size != "256x256" && size != "512x512" && size != "1024x1024") {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid size, please use 256x256 or 512x512 or 1024x1024");
                return null;
            }

            var image = imageGoo.Value;

            string b64img = string.Empty;
            if (image is string imageString) {
                if (File.Exists(imageString)) {
                    var img = Image.FromFile(imageString);
                    if (img == null) {
                        AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Failed to load image");
                        return null;
                    }
                    b64img = Utils.ImageToBase64(img, System.Drawing.Imaging.ImageFormat.Png);
                } else {
                    b64img = imageString;
                }
            } else if (image is Bitmap bmp) {
                b64img = Utils.ImageToBase64(bmp, System.Drawing.Imaging.ImageFormat.Png);
            } else if (image is Image img) {
                b64img = Utils.ImageToBase64(img, System.Drawing.Imaging.ImageFormat.Png);
            }
            if (string.IsNullOrEmpty(b64img)) {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Failed to get image");
                return null;
            }
            var imgBytes = Convert.FromBase64String(b64img);


            var mask = maskGoo.Value;

            string b64mask = string.Empty;
            if (image is string maskString) {
                if (File.Exists(maskString)) {
                    var img = Image.FromFile(maskString);
                    if (img == null) {
                        AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Failed to load mask");
                        return null;
                    }
                    b64mask = Utils.ImageToBase64(img, System.Drawing.Imaging.ImageFormat.Png);
                } else {
                    b64mask = maskString;
                }
            } else if (image is Bitmap bmp) {
                b64mask = Utils.ImageToBase64(bmp, System.Drawing.Imaging.ImageFormat.Png);
            } else if (image is Image img) {
                b64mask = Utils.ImageToBase64(img, System.Drawing.Imaging.ImageFormat.Png);
            }
            if (string.IsNullOrEmpty(b64mask)) {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Failed to get image");
                return null;
            }
            var maskBytes = Convert.FromBase64String(b64mask);

            var request = new OpenAI.GPT3.ObjectModels.RequestModels.ImageEditCreateRequest() {
                Image = imgBytes, Mask = maskBytes, N = count, 
                Size = size, ImageName = "UserImage", MaskName = "UserMask", Prompt = prompt
            };
            return Task.Run(() => service.CreateImageEdit(request), CancelToken);
        }

        protected override void SolveInstanceInPostSolve(IGH_DataAccess DA, ImageCreateResponse result)
        {
            var imgs = new List<GH_ObjectWrapper>();
            var urls = new List<string>();
            foreach (var r in result.Results) { 
                urls.Add(r.Url);
                if (!string.IsNullOrEmpty(r.B64)) {
                    imgs.Add(new GH_ObjectWrapper(Utils.Base64ToImage(r.B64)));
                } else {
                    using (WebClient webClient = new WebClient()) {
                        byte[] data = webClient.DownloadData(r.Url);

                        using (MemoryStream mem = new MemoryStream(data)) {
                            using (var img = Image.FromStream(mem)) {
                                var tempPath = Path.GetTempPath() + Guid.NewGuid() + ".png";
                                img.Save(tempPath);
                                imgs.Add(new GH_ObjectWrapper(img.Clone()));
                                File.Delete(tempPath);
                            }
                        }

                    }
                }
            }
            DA.SetDataList(0, imgs);
            DA.SetDataList(1, urls);
            DA.SetData(2, result);
        }

    }
    #endregion

    #region Expo3
    public class Comp_OpenAIFileUpload : Comp_BaseResponse<FileUploadResponse>
    {
        public override Guid ComponentGuid => new Guid("c4c72855-ddd6-4c48-ae34-3a5557a8a363");
        public override GH_Exposure Exposure => GH_Exposure.tertiary;

        public Comp_OpenAIFileUpload() : base("File Upload", "File Upload", "Upload a file that contains document(s) to be used across various endpoints/features. Currently, the size of all the files uploaded by one organization can be up to 1 GB.") { }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddParameter(new Param_OpenAIService());
            pManager.AddTextParameter("File Path", "Fil", "The file path to upload", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Purpose", "Pur", "Set true for 'fine-tune' purpose", GH_ParamAccess.item, true); 
            AddAskParameter(pManager);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Id", "I", "", GH_ParamAccess.item);
            pManager.AddParameter(new Param_BaseResponse());
        }

        protected override Task<FileUploadResponse> SolveInstanceInPreSolve(IGH_DataAccess DA, out bool ask)
        {
            OpenAIService service = null;
            bool isFinetune = false;
            string filePath = null;
            ask = false;

            if (!DA.GetData(0, ref service) || !DA.GetData(2, ref isFinetune) || !DA.GetData(1, ref filePath)) {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Failed to get some input");
                return null;
            }
            if (!File.Exists(filePath)) {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "File doesn't exist");
                return null;
            }
            var bytes = File.ReadAllBytes(filePath);
            var fileName = Path.GetFileName(filePath);
            return Task.Run(() => service.UploadFile(isFinetune ? "fine-tune" : string.Empty, bytes, fileName), CancelToken);

        }

        protected override void SolveInstanceInPostSolve(IGH_DataAccess DA, FileUploadResponse result)
        {
            DA.SetData(0, result.Id);
            DA.SetData(1, result);
        }
    }

    public class Comp_OpenAIFileList : Comp_BaseResponse<FileListResponse>
    {
        public override Guid ComponentGuid => new Guid("0d517bcd-731f-4c45-9e95-ae01583e1d23");
        public override GH_Exposure Exposure => GH_Exposure.tertiary;

        public Comp_OpenAIFileList() : base("File List", "File List", "Returns a list of files that belong to the user's organization.") { }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddParameter(new Param_OpenAIService());
            AddAskParameter(pManager);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Names", "N", "File names", GH_ParamAccess.list);
            pManager.AddTextParameter("Ids", "I", "File ids", GH_ParamAccess.list);
            pManager.AddTextParameter("Purposes", "P", "File purposes", GH_ParamAccess.list);
            pManager.AddParameter(new Param_BaseResponse());
        }

        protected override Task<FileListResponse> SolveInstanceInPreSolve(IGH_DataAccess DA, out bool ask)
        {
            OpenAIService service = null;
            ask = false;
            if (!DA.GetData(0, ref service)) {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Failed to get some input");
                return null;
            }
            return Task.Run(() => service.ListFile(), CancelToken);
        }

        protected override void SolveInstanceInPostSolve(IGH_DataAccess DA, FileListResponse result)
        {
            var names = new List<string>();
            var ids = new List<string>();
            var purposes = new List<string>();
            foreach (var data in result.Data) {
                names.Add(data.FileName);
                ids.Add(data.Id);
                purposes.Add(data.Purpose);
            }
            DA.SetDataList(0, names);
            DA.SetDataList(1, ids);
            DA.SetDataList(2, purposes);
            DA.SetData(3, result);
        }
    }

    public class Comp_OpenAIDeleteFile : Comp_BaseResponse<FileDeleteResponse>
    {
        public override Guid ComponentGuid => new Guid("8daa60d7-8c4e-480b-8582-8e5d2875545f");
        public override GH_Exposure Exposure => GH_Exposure.tertiary;

        public Comp_OpenAIDeleteFile() : base("Delete File", "Delete File", "Delete a file") { }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddParameter(new Param_OpenAIService());
            pManager.AddIntegerParameter("Id", "Id", "The ID of the file to use for this request", GH_ParamAccess.item);
            AddAskParameter(pManager);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddBooleanParameter("Deleted", "D", "True if success", GH_ParamAccess.item);
            pManager.AddParameter(new Param_BaseResponse());
        }

        protected override Task<FileDeleteResponse> SolveInstanceInPreSolve(IGH_DataAccess DA, out bool ask)
        {
            OpenAIService service = null;
            string id = string.Empty;
            ask = false;
            if (!DA.GetData(0, ref service) || !DA.GetData(1, ref id)) {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Failed to get some input");
                return null;
            }
            return Task.Run(() => service.DeleteFile(id), CancelToken);
        }

        protected override void SolveInstanceInPostSolve(IGH_DataAccess DA, FileDeleteResponse result)
        { 
            DA.SetData(0, result.Deleted);
            DA.SetData(1, result);
        }
    }

    public class Comp_OpenAIRetrieveFile : Comp_BaseResponse<FileResponse>
    {
        public override Guid ComponentGuid => new Guid("9a7b9a00-b9b0-4ed7-9d57-096bd29b2d87");
        public override GH_Exposure Exposure => GH_Exposure.tertiary;

        public Comp_OpenAIRetrieveFile() : base("Retrieve File", "Retrieve File", "Returns information about a specific file.") { }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddParameter(new Param_OpenAIService());
            pManager.AddIntegerParameter("Id", "Id", "The ID of the file to use for this request", GH_ParamAccess.item);
            AddAskParameter(pManager);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("FileName", "N", "The file name", GH_ParamAccess.item);
            //pManager.AddGenericParameter("Bytes", "B", "", GH_ParamAccess.item);
            pManager.AddTextParameter("Purpose", "P", "The purpose", GH_ParamAccess.item);
            pManager.AddParameter(new Param_BaseResponse());
        }

        protected override Task<FileResponse> SolveInstanceInPreSolve(IGH_DataAccess DA, out bool ask)
        {
            OpenAIService service = null;
            string id = string.Empty;
            ask = false;
            if (!DA.GetData(0, ref service) || !DA.GetData(1, ref id)) {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Failed to get some input");
                return null;
            }
            return Task.Run(() => service.RetrieveFile(id), CancelToken);
        }

        protected override void SolveInstanceInPostSolve(IGH_DataAccess DA, FileResponse result)
        {
            DA.SetData(0, result.FileName); 
            DA.SetData(1, result.Purpose);
            DA.SetData(2, result);
        }
    }

    public class Comp_OpenAIFineTune : Comp_BaseResponse<FineTuneResponse>
    {
        public override Guid ComponentGuid => new Guid("96fbc303-38db-4332-9d75-32b4784180f5");
        public override GH_Exposure Exposure => GH_Exposure.tertiary;

        public Comp_OpenAIFineTune() : base("Fine Tune", "Fine Tune", "Manage fine-tuning jobs to tailor a model to your specific training data.") { }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddParameter(new Param_OpenAIService());
            pManager.AddTextParameter("Model", "Mod", "The name of the base model to fine-tune. You can select one of \"ada\", \"babbage\", \"curie\", \"davinci\", or a fine-tuned model created after 2022-04-21.", GH_ParamAccess.item, "text-davinci-003");
            pManager.AddTextParameter("File Id", "Id", "The ID of an uploaded file that contains training data.", GH_ParamAccess.item);
          AddAskParameter(pManager);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Fine Tuned Model", "M", "The resulting fine-tunned model", GH_ParamAccess.list); 
            pManager.AddParameter(new Param_BaseResponse());
        }

        protected override Task<FineTuneResponse> SolveInstanceInPreSolve(IGH_DataAccess DA, out bool ask)
        {
            OpenAIService service = null;
            string model = null;
            string id = null;
            ask = false;

            if (!DA.GetData(0, ref service) || !DA.GetData(1, ref model) || !DA.GetData(2, ref id) ) {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Failed to get some input");
                return null;
            }

            var request = new OpenAI.GPT3.ObjectModels.RequestModels.FineTuneCreateRequest() {
                Model = model, TrainingFile = id // TODO add more parameters
            };
            return Task.Run(() => service.CreateFineTune(request), CancelToken);

        }

        protected override void SolveInstanceInPostSolve(IGH_DataAccess DA, FineTuneResponse result)
        {
            DA.SetData(0, result.FineTunedModel);
            // TODO add more parameters
            DA.SetData(1, result);
        }
    }

    #endregion

    #region Expo4
    public class Comp_ExplodeResponse : Comp_Base
    {
        public override Guid ComponentGuid => new Guid("ea34ac54-f551-40e7-8c98-d2cbe2ffd27f");
        public override GH_Exposure Exposure => GH_Exposure.quarternary;

        public Comp_ExplodeResponse() : base("Explode Response", "ExpResponse", "Explode a BaseResponse into its properties") { }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddParameter(new Param_BaseResponse());
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddBooleanParameter("Successful", "S", "", GH_ParamAccess.item);
            pManager.AddTextParameter("Id", "I", "", GH_ParamAccess.item);
            pManager.AddTextParameter("Model", "M", "", GH_ParamAccess.item);
            pManager.AddTextParameter("Object Type Name", "N", "", GH_ParamAccess.item);
            pManager.AddTextParameter("Error", "E", "", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            BaseResponse response = null;

            if (!DA.GetData(0, ref response))
                return;

            DA.SetData(0, response.Successful);
            if(response is IOpenAiModels.IId id) {
                DA.SetData(1, id.Id);
            }
            if (response is IOpenAiModels.IModel model) {
                DA.SetData(2, model.Model);
            } 
            DA.SetData(3, response.ObjectTypeName);
            DA.SetData(4, response.Error?.Message);
        }
    }
    #endregion
     
}