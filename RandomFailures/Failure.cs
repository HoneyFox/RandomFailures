using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace RandomFailures
{
	public class Failure
	{
		public static int Compare(Failure a, Failure b)
		{
			if (a.GetType().Equals(b.GetType()))
				if (a.parentPart == b.parentPart && a.parentPartModule == b.parentPartModule)
					return 0;

			return 1;
		}

		public string failureName = "Unknown failure";
		public Part parentPart = null;
		public PartModule parentPartModule = null;

		public bool executeOnce = true;
		public bool hasTriggered = false;
		public double universeTimeCreated = 0;

		public double timeElapsed
		{
			get { return Planetarium.GetUniversalTime() - universeTimeCreated; }
		}

		public virtual void SetParentPart(Part part)
		{
			parentPart = part;
		}

		public virtual void SetParentModule(PartModule module)
		{
			parentPart = module.part;
			parentPartModule = module;
		}

		public virtual void OnLoad(string data, Part part)
		{
			string[] sections = data.Split(',');
			if (sections[0] != this.GetType().Name)
			{
				Debug.Log("RandomFailures: Failure type doesn't match!");
			}

			universeTimeCreated = Convert.ToDouble(sections[1]);
			hasTriggered = Convert.ToBoolean(sections[2]);

			SetParentPart(part);
		}

		public virtual string OnSave()
		{
			return this.GetType().Name + "," + universeTimeCreated.ToString("F1") + "," + hasTriggered.ToString();
		}

		public virtual bool OnJudge()
		{
			// return true to make it happen.
			if(executeOnce)
				return false;
			if (hasTriggered && !executeOnce)
				return true;
			return false;
		}

		public virtual void Execute()
		{ 
			// Do nothing here.
			if(parentPart != null)
				Debug.Log("RandomFailures: " + failureName + " occured on " + parentPart.partInfo.title + "!");
		}
	}
}
