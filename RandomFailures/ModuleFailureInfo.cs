using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace RandomFailures
{
	public class ModuleFailureInfo : PartModule
	{
		[KSPField(isPersistant = true)]
		public string m_failureInfo = "NULL";

		private StartState startState = StartState.None;

		public List<Failure> m_failures = new List<Failure>();

		public override void OnStart(StartState state)
		{
			base.OnStart(state);
			startState = state;
			if (state != StartState.Editor && state != StartState.None)
			{
				string[] failureStrs = m_failureInfo.Split(new char[]{'>', '<'}, StringSplitOptions.RemoveEmptyEntries);
				foreach (string failureStr in failureStrs)
				{
					string failureType = failureStr.Split(',')[0];
					try
					{
						Type type = Type.GetType(failureType);
						object newFailure = type.GetConstructor(Type.EmptyTypes).Invoke(new object[0]);
						if (newFailure is Failure)
						{
							(newFailure as Failure).OnLoad(failureStr, part);
							m_failures.Add(newFailure as Failure);
						}
						else
						{
							Debug.Log("RandomFailures: Error: the constructed object is not an object of type \"Failure\".");
						}
					}
					catch (Exception e)
					{
						Debug.Log(e.Message);
					}
				}
			}
		}

		public override void OnUpdate()
		{
			base.OnUpdate();

			if (RandomFailures.s_Singleton.m_enabled)
			{
				foreach (Failure failure in m_failures)
				{
					if (RandomFailures.s_Singleton.m_counter % RandomFailures.s_Singleton.m_interval == 1 && RandomFailures.s_Singleton.m_overallProbability >= UnityEngine.Random.Range(0.0f, 1.0f))
					{
						if (failure.executeOnce == false && failure.hasTriggered)
						{
							failure.Execute();
						}
						else if (failure.OnJudge() == true)
						{
							failure.Execute();
						}
					}
				}
			}
		}

		public override void OnSave(ConfigNode node)
		{
			if (startState != StartState.Editor && startState != StartState.None)
				this.m_failureInfo = GenerateFailureInfoStr();
			else
				this.m_failureInfo = "NULL";

			base.OnSave(node);
		}

		private string GenerateFailureInfoStr()
		{
			string result = "";
			foreach (Failure failure in m_failures)
			{ 
				result += "<";
				result += failure.OnSave();
				result += ">";
			}

			return result;
		}

		public bool Contains(Comparison<Failure> comparison, Failure failure)
		{
			foreach (Failure m_failure in m_failures)
			{
				if (comparison(failure, m_failure) == 0)
					return true;
			}
			return false;
		}
	}
}
