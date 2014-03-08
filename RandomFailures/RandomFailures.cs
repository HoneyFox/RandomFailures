using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace RandomFailures
{
	public class RandomFailures : MonoBehaviour
	{
		public static RandomFailures s_Singleton = null;

		public void OnAwake()
		{
			DontDestroyOnLoad(s_Singleton);
		}

		public void Start()
		{
			MonoBehaviour.print("Random Failures System Started!");
		}

		/// <summary>
		/// Random Failures System Logic
		/// </summary>

		public bool m_enabled = true;

		public int m_counter = 0;

		public int m_interval = 10;
		public float m_overallProbability = 0.25f;

		public Vessel currentVessel = null;
		
		public void Update()
		{
			if (s_Singleton != this) return;

			// Handle input here.
			if (Input.GetKey(KeyCode.RightControl) && Input.GetKeyDown(KeyCode.F))
			{
				m_enabled = !m_enabled;
				MonoBehaviour.print("Random Failures System State: " + m_enabled.ToString());
			}

			if (MapView.fetch != null)
				MapView.fetch.max3DlineDrawDist = 50000f;

			if (m_enabled && FlightGlobals.fetch != null && FlightGlobals.fetch.activeVessel != null)
			{
				if (m_counter == 13)
				{
					MonoBehaviour.print("Random Failures System Running...");
					foreach (Part part in FlightGlobals.fetch.activeVessel.parts)
					{
						if (part.Modules.Contains("ModuleFailureInfo"))
						{
							ModuleFailureInfo failureInfo = (part.Modules["ModuleFailureInfo"] as ModuleFailureInfo);
							//foreach (Failure failure in failureInfo.m_failures)
							//{
							//    if(failure.parentPart != null)
							//        Debug.Log(failure.parentPart.partName + ": " + failure.GetType().Name);								
							//}
						}
					}
				}

				m_counter = (m_counter + 1) % 500;

				// The first random failure logic module.
				if (FlightGlobals.fetch.activeVessel.isEVA == false)
				{
					if (FlightGlobals.fetch.activeVessel != currentVessel)
					{
						currentVessel = FlightGlobals.fetch.activeVessel;
						analyzeCurrentVesselPotentialFailures();
					}
					else
					{
						if (currentVessel != null)
							checkCurrentVesselPotentialFailures();
					}
				}
			}
			else
			{
				currentVessel = null;
			}
		}

		private void analyzeCurrentVesselPotentialFailures()
		{
			Debug.Log("RandomFailures: Starting analyzing...");
			foreach (Part part in FlightGlobals.fetch.activeVessel.parts)
			{
				if (part.Modules.Contains("ModuleFailureInfo") == false)
				{
					Debug.Log("RandomFailures: this part doesn't contain ModuleFailureInfo.");
					continue;
				}

				ModuleFailureInfo failureInfo = (part.Modules["ModuleFailureInfo"] as ModuleFailureInfo);

				//if (failureInfo.m_failureInfo != "NULL")
				//{
				//    Debug.Log("RandomFailures: this part has been analyzed before.");
				//    continue;
				//}

				if (part.Modules.Contains("ModuleDecouple"))
				{
					Debug.Log("RandomFailures: Found a decoupler!");
					DecouplerFailure newFailure = new DecouplerFailure();
					newFailure.universeTimeCreated = Planetarium.GetUniversalTime();
					newFailure.SetParentModule(part.Modules["ModuleDecouple"]);
					if (!failureInfo.Contains(DecouplerFailure.Compare, newFailure))
					{
						failureInfo.m_failures.Add(newFailure);
					}
				}
				if (part.Modules.Contains("ModuleAnchoredDecoupler"))
				{
					Debug.Log("RandomFailures: Found a decoupler!");
					DecouplerFailure newFailure = new DecouplerFailure();
					newFailure.universeTimeCreated = Planetarium.GetUniversalTime(); 
					newFailure.SetParentModule(part.Modules["ModuleAnchoredDecoupler"]);
					if (!failureInfo.Contains(DecouplerFailure.Compare, newFailure))
					{
						failureInfo.m_failures.Add(newFailure);
					}
				}
				if (part.Modules.Contains("ModuleEngines"))
				{
					Debug.Log("RandomFailures: Found an engine!");
					EngineExplosionFailure newFailure = new EngineExplosionFailure();
					newFailure.universeTimeCreated = Planetarium.GetUniversalTime(); 
					newFailure.SetParentModule(part.Modules["ModuleEngines"]);
					if (!failureInfo.Contains(EngineExplosionFailure.Compare, newFailure))
					{
						failureInfo.m_failures.Add(newFailure);
					}
				}
				if (part.Modules.Contains("ModuleEnginesFX"))
				{
					Debug.Log("RandomFailures: Found an engine!");
					EngineExplosionFailure newFailure = new EngineExplosionFailure();
					newFailure.universeTimeCreated = Planetarium.GetUniversalTime();
					newFailure.SetParentModule(part.Modules["ModuleEnginesFX"]);
					if (!failureInfo.Contains(EngineExplosionFailure.Compare, newFailure))
					{
						failureInfo.m_failures.Add(newFailure);
					}
				}
				if (part.Modules.Contains("ModuleEngines"))
				{
					Debug.Log("RandomFailures: Found an engine!");
					EngineVibrationFailure newFailure = new EngineVibrationFailure();
					newFailure.universeTimeCreated = Planetarium.GetUniversalTime();
					newFailure.SetParentModule(part.Modules["ModuleEngines"]);
					if (!failureInfo.Contains(EngineVibrationFailure.Compare, newFailure))
					{
						failureInfo.m_failures.Add(newFailure);
					}
				}
				if (part.Modules.Contains("ModuleEnginesFX"))
				{
					Debug.Log("RandomFailures: Found an engine!");
					EngineVibrationFailure newFailure = new EngineVibrationFailure();
					newFailure.universeTimeCreated = Planetarium.GetUniversalTime();
					newFailure.SetParentModule(part.Modules["ModuleEnginesFX"]);
					if (!failureInfo.Contains(EngineVibrationFailure.Compare, newFailure))
					{
						failureInfo.m_failures.Add(newFailure);
					}
				}
				if (part.Resources.Count != 0)
				{
					Debug.Log("RandomFailures: Found some resources!");
					foreach (PartResource resource in part.Resources.list)
					{
						ResourceLeakageFailure newFailure = new ResourceLeakageFailure();
						newFailure.universeTimeCreated = Planetarium.GetUniversalTime(); 
						newFailure.SetParentPart(part);
						newFailure.SetResource(resource.resourceName);
						if (newFailure.parentPart != null)
						{
							if (!failureInfo.Contains(ResourceLeakageFailure.Compare, newFailure))
							{
								failureInfo.m_failures.Add(newFailure);
								Debug.Log("RandomFailures: Found a valid resource that may leak!");
							}
						}
					}
				}
			}
		}

		private void checkCurrentVesselPotentialFailures()
		{
			foreach (Part part in currentVessel.parts)
			{
				if (part.Modules.Contains("ModuleFailureInfo"))
				{ 
					ModuleFailureInfo failureInfo = (part.Modules["ModuleFailureInfo"] as ModuleFailureInfo);
					for (int i = 0; i < failureInfo.m_failures.Count; ++i)
					{
						if (failureInfo.m_failures[i].parentPart == null || failureInfo.m_failures[i].parentPart.vessel == null)
						{
							Debug.Log("RandomFailures: the part/vessel no longer exists.");
							failureInfo.m_failures.RemoveAt(i);
							i--;
						}
					}
				}
			}
		}
	}


	public class RandomFailuresUnitTest : KSP.Testing.UnitTest
	{
		public RandomFailuresUnitTest()
		{
			GameObject gameObj = new GameObject("RandomFailures", typeof(RandomFailures));
			RandomFailures.s_Singleton = gameObj.GetComponent<RandomFailures>();
			GameObject.DontDestroyOnLoad(gameObj);
		}
	}
}
