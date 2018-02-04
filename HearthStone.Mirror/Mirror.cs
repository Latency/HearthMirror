using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using HearthStone.Mirror.Mono;


namespace HearthStone.Mirror
{
	internal class Mirror
	{
		public string ImageName { get; set; }
		public bool Active => _process != null;

	    public Dictionary<string, Type> RefTypes = new Dictionary<string, Type>();

		Process _process;
		public Process Proc => _process ?? (_process = Process.GetProcessesByName(ImageName).FirstOrDefault());

		private ProcessView _view;
		private dynamic _root;

		public ProcessView View
		{
			get
			{
				if(Proc == null)
					return null;
				return _view ?? (_view = new ProcessView(Proc));
			}
		}

		internal void Clean()
		{
			_process = null;
			_view = null;
			_root = null;
		}

		public dynamic Root
		{
			get
			{
				if(_root != null)
					return _root;
				var view = View;
				if(view == null)
					return null;
				var rootDomainFunc = view.GetExport("mono_get_root_domain");
				var buffer = new byte[6];
				view.ReadBytes(buffer, 0, 6, rootDomainFunc);
				if(buffer[0] != 0xa1 || buffer[5] != 0xc3)
					return null;
				var pRootDomain = BitConverter.ToUInt32(buffer, 1);
				view.ReadBytes(buffer, 0, 4, pRootDomain);
				var rootDomain = BitConverter.ToUInt32(buffer, 0);
				view.ReadBytes(buffer, 0, buffer.Length, rootDomain);
				var next = view.ReadUint(rootDomain + Offsets.MonoDomain_domain_assemblies);
				uint pImage = 0;
				while(next != 0)
				{
					var data = view.ReadUint(next);
					next = view.ReadUint(next + 4);
					var name = view.ReadCString(view.ReadUint(data + Offsets.MonoAssembly_name));
					if(name == "Assembly-CSharp")
					{
						pImage = view.ReadUint(data + Offsets.MonoAssembly_image);
						break;
					}
				}
				 _root = new MonoImage(view, pImage);

				// Generate class entities.
				//var collection = ReflectionExtensions.GetMemberValue(_root, "_classes");
			    //foreach (var cls in collection) {
				//	var properties = from field in ReflectionExtensions.GetMemberValue(cls.Value, "Fields") as MonoClassField[]
				//		  where !field.Name.StartsWith("<") && !field.Name.StartsWith("HutongGames.")
				//		  orderby field.Offset
				//		  select field.Name;
				//DynamicClass.CreateType(this, cls.Key, properties);
				//}

				return _root;
			}
		}
	}
}
