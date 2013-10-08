using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace RandomFailures
{
	public class EngineExplosionFailure : Failure
	{
		public new static int Compare(Failure a, Failure b)
		{
			return Failure.Compare(a, b);
		}

		public EngineExplosionFailure()
		{
			failureName = "Engine exploded";
		}

		public override void SetParentModule(PartModule module)
		{
			if (module is ModuleEngines)
			{
				parentPart = module.part;
				parentPartModule = module;
			}
			else
			{
				parentPart = null;
				parentPartModule = null;
			}
		}

		public override void OnLoad(string data, Part part)
		{
			base.OnLoad(data, part); 

			if(part.Modules.Contains("ModuleEngines"))
				SetParentModule(part.Modules["ModuleEngines"]);
		}

		public override bool OnJudge()
		{
			if (parentPart == null) return false;

			Debug.Log("RandomFailures: EngineExplosionFailure::OnJudge(): temparature = " + parentPart.temperature.ToString());
			Debug.Log("RandomFailures: EngineExplosionFailure::OnJudge(): maxTemp = " + parentPart.maxTemp.ToString());

			if (parentPart.temperature > parentPart.maxTemp * 0.75f)
			{
				if (UnityEngine.Random.Range(0.0f, 1.0f) < ((parentPart.temperature / parentPart.maxTemp) - 0.75f) * 0.0001f)
				{
					hasTriggered = true;
					return true;
				}
			}
			return false;
		}

		public override void Execute()
		{
			if (parentPart == null) return;
			
			Debug.Log("RandomFailures: " + failureName + " on " + parentPart.partName + "!");
			parentPart.explode();
		}
	}
}
