using System;
using System.Collections.Generic;

namespace RaBehaviourSO
{
	/// <summary>
	/// This class is to manage <see cref="RaBehaviourSOBase"/>
	/// Use <see cref="Register(object)"/> when you enter a scene and <see cref="Unregister(object)"/> when you exit it to initialize and deinitialize the states
	/// If you want to preserve the <see cref="RaBehaviourSOBase"/>, Use unregister only when the next scene has chance to Register. 
	/// Or call it on the OnDestroy of an object marked with "Don't Destroy On Load" only to clean the <see cref="RaBehaviourSOBase"/> when you exit play mode
	/// </summary>
	public class RaBehaviourSOController : IDisposable
	{
		private readonly HashSet<object> _users;

		public RaBehaviourSOBase[] Behaviours
		{
			get;
		}

		public RaBehaviourSOController(RaBehaviourSOBase[] behaviours)
		{
			Behaviours = behaviours;
			_users = new HashSet<object>();
		}

		/// <summary>
		/// Initialization of the <see cref="RaBehaviourSOBase"/> will occur when the first user has been registered
		/// </summary>
		/// <param name="user">A scene or object reference which acts as the user of the Behaviours collection</param>
		public void Register(object user)
		{
			_users.Add(user);
			
			try
			{
				ForEach(x => x.Initialize());
				ForEach(x => x.Initialized());
			}
			catch(Exception e)
			{
				Dispose();
				throw e;
			}
		}

		/// <summary>
		/// Deinitliazation of the <see cref="RaBehaviourSOBase"/> will occur when the last user has been unregistered
		/// </summary>
		/// <param name="user">A scene or object reference which acts as the user of the Behaviours collection</param>
		public void Unregister(object user)
		{
			_users.Remove(user);
			if(_users.Count == 0)
			{
				ForEach(x => x.Deinitialize());
			}
		}

		public List<T> GetBehaviours<T>(Predicate<T> predicate)
				where T : RaBehaviourSOBase
		{
			List<T> returnValue = new List<T>();
			for(int i = 0; i < Behaviours.Length; i++)
			{
				var rawBehaviour = Behaviours[i];
				if(rawBehaviour is T castedBahaviour && (predicate == null || predicate(castedBahaviour)))
				{
					returnValue.Add(castedBahaviour);
				}
			}
			return returnValue;
		}

		public bool TryGetBehaviour<T>(out T behaviour, Predicate<T> predicate = null)
			where T : RaBehaviourSOBase
		{
			for(int i = 0; i < Behaviours.Length; i++)
			{
				var rawBehaviour = Behaviours[i];
				if(rawBehaviour is T castedBahaviour && (predicate == null || predicate(castedBahaviour)))
				{
					behaviour = castedBahaviour;
					return true;
				}
			}

			behaviour = default;
			return false;
		}

		public void ForEach<T>(Action<T> action)
			where T : RaBehaviourSOBase
		{
			for(int i = 0; i < Behaviours.Length; i++)
			{
				var rawBehaviour = Behaviours[i];
				if(rawBehaviour is T castedBahaviour)
				{
					action(castedBahaviour);
				}
			}
		}

		public void ForEach(Action<RaBehaviourSOBase> action)
		{
			for(int i = 0; i < Behaviours.Length; i++)
			{
				action(Behaviours[i]);
			}
		}

		/// <summary>
		/// This will clear all users and call Deinitialize on all Behaviours
		/// </summary>
		public void ForceDeinitialization()
		{
			_users.Clear();
			ForEach(x => x.Deinitialize());
		}

		/// <summary>
		/// This will clear all users and Dispose all Behaviours
		/// </summary>
		public void Dispose()
		{
			_users.Clear();
			ForEach(x => x.Dispose());
		}
	}
}