using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace RandomFailures
{
	public class ResourceLeakageFailure : Failure
	{
		public new static int Compare(Failure a, Failure b)
		{
			if (Failure.Compare(a, b) == 0)
				if ((a as ResourceLeakageFailure).resourceName == (b as ResourceLeakageFailure).resourceName)
					return 0;

			return 1;
		}

		public string resourceName = "";
		public double severity = 1.0;

		public ResourceLeakageFailure()
		{
			failureName = "Resource is leaking";
			executeOnce = false;
		}

		public override void SetParentPart(Part part)
		{
			parentPart = part;
		}

		public void SetResource(string resource)
		{
			if (parentPart != null)
			{
				// Here we assume that it's some sort of liquid resource.
				if (parentPart.Resources[resource].info.density > 0.000001 && parentPart.Resources[resource].info.resourceFlowMode != ResourceFlowMode.NO_FLOW)
				{
					resourceName = resource;
					failureName = "Resource " + resourceName + " is leaking";
				}
				else
				{ 
					parentPart = null;
				}
			}
		}

		public override void OnLoad(string data, Part part)
		{
			base.OnLoad(data, part);

			string[] sections = data.Split(',');

			SetParentPart(part);
			SetResource(sections[3]);
			severity = Convert.ToSingle(sections[4]);
		}

		public override string OnSave()
		{
			return this.GetType().Name + "," + universeTimeCreated.ToString() + "," + hasTriggered.ToString() + "," + resourceName + "," + severity.ToString("F2");
		}

		public override bool OnJudge()
		{
			if (parentPart == null) return false;

			if (parentPart.Resources.Contains(resourceName))
			{
				//Debug.Log("ResourceLeakageFailure::OnJudge(): geeForce_immediate = " + parentPart.vessel.geeForce_immediate.ToString());
				//Debug.Log("ResourceLeakageFailure::OnJudge(): surfaceAreas = " + parentPart.surfaceAreas.ToString());
				
				float totalVolume = Convert.ToSingle(parentPart.Resources[resourceName].maxAmount);
				float surfaceArea = Mathf.Pow(totalVolume, 2.0f / 3.0f);
				float dryMass = parentPart.mass;
				//Debug.Log("ResourceLeakageFailure::OnJudge(): surfAreaByVolume = " + surfaceArea.ToString());

				if (hasTriggered == true)
					return true;

				if (parentPart.vessel.geeForce_immediate > 1500.0f * dryMass / surfaceArea)
				{
					float probability = (Convert.ToSingle(parentPart.vessel.geeForce_immediate) / (1500.0f * dryMass / surfaceArea) - 1.0f) * 0.5f;
					probability *= (1.0f + Convert.ToSingle(Math.Log10(timeElapsed / 10000.0 + 1)));
					if (UnityEngine.Random.Range(0.0f, 1.0f) < probability)
					{
						severity = UnityEngine.Random.Range(0.0f, 1.0f);
						hasTriggered = true;
						Debug.Log(failureName + " on " + parentPart.partInfo.title);
						return true;
					}
				}
			}
			return false;
		}

		public override void Execute()
		{
			if (parentPart == null) return;

			double amountDecrement = parentPart.Resources[resourceName].amount * 0.0001 * severity;
			parentPart.RequestResource(resourceName, amountDecrement);
		}
	}
}
