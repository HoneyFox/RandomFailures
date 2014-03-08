using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace RandomFailures
{
	public class DecouplerFailure : Failure
	{
		public new static int Compare(Failure a, Failure b)
		{
			return Failure.Compare(a, b);
		}

		public DecouplerFailure()
		{
			failureName = "Decoupler unintended triggered";
		}

		public override void SetParentModule(PartModule module)
		{
			parentPart = module.part;
			parentPartModule = module;
		}

		public override void OnLoad(string data, Part part)
		{
			base.OnLoad(data, part);

			string[] sections = data.Split(',');

			if (part.Modules.Contains(sections[3]))
			{ 
				SetParentModule(part.Modules[sections[3]]);
			}
		}

		public override bool OnJudge()
		{
			if (parentPart == null) return false;

			//Debug.Log("RandomFailures: EngineExplosionFailure::OnJudge(): temparature = " + parentPart.temperature.ToString());
			//Debug.Log("RandomFailures: EngineExplosionFailure::OnJudge(): maxTemp = " + parentPart.maxTemp.ToString());

			if (parentPart.temperature > parentPart.maxTemp * 0.75f)
			{
				if (UnityEngine.Random.Range(0.0f, 1.0f) < ((parentPart.temperature / parentPart.maxTemp) - 0.7f) * 0.00001f)
					return true;
			}
			return false;
		}

		public override void Execute()
		{
			if (parentPart == null) return;
			
			Debug.Log("RandomFailures: " + failureName + " on " + parentPart.partInfo.title + "!");
			parentPart.force_activate();
		}
	}
}
