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
	public class RaBehaviourSOController<TRaBehaviourSO> : IDisposable
		where TRaBehaviourSO : RaBehaviourSOBase
	{
		public delegate void BehaviourHandler(TRaBehaviourSO behaviourSO);

		private readonly HashSet<object> _users;
		private BehaviourHandler _onInit;
		private BehaviourHandler _onDeinit;

		public TRaBehaviourSO[] Behaviours
		{
			get;
		}

		public RaBehaviourSOController(TRaBehaviourSO[] behaviours, BehaviourHandler onInit = null, BehaviourHandler onDeinit = null)
		{
			Behaviours = behaviours;

			_onInit = onInit;
			_onDeinit = onDeinit;
			
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
				PerformInit();
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
				PerformDeinit();
			}
		}

		public List<T> GetBehaviours<T>(Predicate<T> predicate)
				where T : TRaBehaviourSO
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
			where T : TRaBehaviourSO
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
			where T : TRaBehaviourSO
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

		public void ForEach(Action<TRaBehaviourSO> action)
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
			PerformDeinit();
		}

		/// <summary>
		/// This will clear all users and Dispose all Behaviours
		/// </summary>
		public void Dispose()
		{
			ForceDeinitialization();
			ForEach(x => x.Dispose());

			_onInit = null;
			_onDeinit = null;
		}

		private void PerformInit()
		{
			if(_onInit != null)
			{
				ForEach(x =>
				{
					_onInit(x);
					x.Initialize();
				});
			}
			else
			{
				ForEach(x => x.Initialize());
			}

			ForEach(x => x.Initialized());
		}

		private void PerformDeinit()
		{
			if(_onDeinit != null)
			{
				ForEach(x =>
				{
					x.Deinitialize();
					_onDeinit(x);
				});
			}
			else
			{
				ForEach(x => x.Deinitialize());
			}
		}
	}
}