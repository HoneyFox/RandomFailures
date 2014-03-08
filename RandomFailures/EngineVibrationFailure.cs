using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace RandomFailures
{
	public class EngineVibrationFailure : Failure
	{
		public new static int Compare(Failure a, Failure b)
		{
			return Failure.Compare(a, b);
		}

		//private int cycle = 0;

		public EngineVibrationFailure()
		{
			failureName = "Engine vibration exceeded";
		}
		
		public override void SetParentModule(PartModule module)
		{
			if (module is ModuleEngines || module is ModuleEnginesFX)
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

			if (part.Modules.Contains("ModuleEngines"))
				SetParentModule(part.Modules["ModuleEngines"]);
			else if (part.Modules.Contains("ModuleEnginesFX"))
				SetParentModule(part.Modules["ModuleEnginesFX"]);
		}

		private Vector2 CalculateRisk(float distance, float thrust, float ownThrust, bool isOwnNozzle, int ownNozzles = 0)
		{
			Vector2 ret = new Vector2(0.0f, 0.0f);
			
			float resonateFactor = Mathf.Max(thrust, ownThrust) / Mathf.Min(thrust, ownThrust); // 计算推力比值
			resonateFactor = 1.0f / resonateFactor * Mathf.Pow(Mathf.Cos(resonateFactor * Mathf.PI), 4.0f); // 计算推力共振系数
			if (isOwnNozzle == true) resonateFactor *= 0.05f; // 同一个发动机的喷口这部分应该基本不考虑

			/*
			if(ownNozzles > 0) resonateFactor /= ownNozzles; // 对同一个发动机的多个喷口，需要打折扣
			if (ownNozzles > 0) resonateFactor *= 0.05f; // 再打一次更猛的折扣
			float result = Mathf.Sqrt(thrust) / Mathf.Sqrt(distance) * (0.1f + resonateFactor * 0.9f); // 最后结果
			//Debug.Log("Vibration: " + result.ToString());
			result *= 0.000008f; // 整体概率系数
			*/

			float result = (0.1f + resonateFactor * 0.9f) * Mathf.Sqrt(thrust) * Mathf.Pow(0.85f, distance) * 0.000003f;
			result /= Mathf.Max(ownNozzles, 1);
			ret.x = result;
			
			float ownNozzleFactor = 0.0000036f * Mathf.Sqrt(ownThrust * Mathf.Max(ownNozzles, 1)) * Mathf.Pow(1.12f, Mathf.Max(ownNozzles, 1) - 1);
			if(ownNozzles > 1)
				ownNozzleFactor /= (Mathf.Max(ownNozzles, 1) - 1);
			if (isOwnNozzle == false)
				ownNozzleFactor = ownNozzleFactor * 0.01f; // 不同发动机的喷口这部分应该不考虑
			result = ownNozzleFactor / Mathf.Max(ownNozzles, 1);
			ret.y = result;

			return ret;
		}

		private float CalculatePossibility(List<Vector2> otherNozzles)
		{
			Vector2 possibility = new Vector2(0.0f, 0.0f);
			foreach(Vector2 vec in otherNozzles)
			{
				possibility += vec;
			}
			Debug.Log("Total(" + parentPart.partInfo.title + "): " + possibility.x.ToString("F8") + ", " + possibility.y.ToString("F8"));
			return possibility.x + possibility.y;
		}

		public override bool OnJudge()
		{
			if (parentPart == null) return false;

			//if (cycle == 6)
			//{
			//    cycle = 0;
			//}
			//else
			//{
			//    cycle++;
			//    return false;
			//}

			// Now we need to find all nearby nozzles.
			List<Vector2> otherNozzles = new List<Vector2>();
					
			if (parentPartModule is ModuleEngines)
			{
				ModuleEngines ownEngine = parentPartModule as ModuleEngines;
				
				// Engines not working won't be affected.
				if (ownEngine.finalThrust == 0.0f) return false;

				foreach (Transform nozzleTransform in ownEngine.thrustTransforms)
				{
					// These are nozzles on the same engine.
					foreach (Transform ownNozzle in ownEngine.thrustTransforms)
					{
						if (ownNozzle == nozzleTransform) continue;
						otherNozzles.Add
						(
							CalculateRisk
							(
								(ownNozzle.position - nozzleTransform.position).magnitude,
								ownEngine.finalThrust / ownEngine.thrustTransforms.Count,
								ownEngine.finalThrust / ownEngine.thrustTransforms.Count,
								true,
								ownEngine.thrustTransforms.Count
							)
						);
					}

					foreach (Part part in this.parentPart.vessel.Parts)
					{
						if (part == this.parentPart) continue;

						if (part.Modules.Contains("ModuleEngines"))
						{
							ModuleEngines engine = part.Modules["ModuleEngines"] as ModuleEngines;
							if (engine.finalThrust == 0) continue;
							foreach (Transform nozzle in engine.thrustTransforms)
							{
								otherNozzles.Add
								(
									CalculateRisk
									(
										(nozzle.position - nozzleTransform.position).magnitude,
										engine.finalThrust / engine.thrustTransforms.Count,
										ownEngine.finalThrust / ownEngine.thrustTransforms.Count,
										false,
										ownEngine.thrustTransforms.Count
									)
								);
							}
						}
						else if (part.Modules.Contains("ModuleEnginesFX"))
						{
							ModuleEnginesFX engine = part.Modules["ModuleEnginesFX"] as ModuleEnginesFX;
							if (engine.finalThrust == 0) continue;
							foreach (Transform nozzle in engine.thrustTransforms)
							{
								otherNozzles.Add
								(
									CalculateRisk
									(
										(nozzle.position - nozzleTransform.position).magnitude,
										engine.finalThrust / engine.thrustTransforms.Count,
										ownEngine.finalThrust / ownEngine.thrustTransforms.Count,
										false,
										ownEngine.thrustTransforms.Count
									)
								);
							}
						}
					}
				}
			}
			else
			{
				ModuleEnginesFX ownEngine = parentPartModule as ModuleEnginesFX;

				// Engines not working won't be affected.
				if (ownEngine.finalThrust == 0.0f) return false;

				foreach (Transform nozzleTransform in ownEngine.thrustTransforms)
				{
					// These are nozzles on the same engine.
					foreach (Transform ownNozzle in ownEngine.thrustTransforms)
					{
						if (ownNozzle == nozzleTransform) continue;
						otherNozzles.Add
						(
							CalculateRisk
							(
								(ownNozzle.position - nozzleTransform.position).magnitude,
								ownEngine.finalThrust / ownEngine.thrustTransforms.Count,
								ownEngine.finalThrust / ownEngine.thrustTransforms.Count,
								true,
								ownEngine.thrustTransforms.Count
							)
						);
					}

					foreach (Part part in this.parentPart.vessel.Parts)
					{
						if (part == this.parentPart) continue;

						if (part.Modules.Contains("ModuleEngines"))
						{
							ModuleEngines engine = part.Modules["ModuleEngines"] as ModuleEngines;
							if (engine.finalThrust == 0) continue;
							foreach (Transform nozzle in engine.thrustTransforms)
							{
								otherNozzles.Add
								(
									CalculateRisk
									(
										(nozzle.position - nozzleTransform.position).magnitude,
										engine.finalThrust / engine.thrustTransforms.Count,
										ownEngine.finalThrust / ownEngine.thrustTransforms.Count,
										false,
										ownEngine.thrustTransforms.Count
									)
								);
							}
						}
						else if (part.Modules.Contains("ModuleEnginesFX"))
						{
							ModuleEnginesFX engine = part.Modules["ModuleEnginesFX"] as ModuleEnginesFX;
							if (engine.finalThrust == 0) continue;
							foreach (Transform nozzle in engine.thrustTransforms)
							{
								otherNozzles.Add
								(
									CalculateRisk
									(
										(nozzle.position - nozzleTransform.position).magnitude,
										engine.finalThrust / engine.thrustTransforms.Count,
										ownEngine.finalThrust / ownEngine.thrustTransforms.Count,
										false,
										ownEngine.thrustTransforms.Count
									)
								);
							}
						}
					}
				}
			}

			if (UnityEngine.Random.Range(0.0f, 1.0f) < CalculatePossibility(otherNozzles))
			{
				hasTriggered = true;
				return true;
			}
			return false;
		}

		public override void Execute()
		{
			if (parentPart == null) return;

			Debug.Log("RandomFailures: " + failureName + " on " + parentPart.partInfo.title + "!");
			parentPart.explode();
		}
	}
}
