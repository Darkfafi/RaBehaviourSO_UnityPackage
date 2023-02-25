using System;
using System.Collections.Generic;
using UnityEngine;

namespace RaBehaviourSO
{
	/// <summary>
	/// This class can only be controlled by putting it within a <see cref="RaBehaviourSOController"/>
	/// </summary>
	public abstract class RaBehaviourSOBase : ScriptableObject
	{
		[SerializeField]
		private ScriptableObject[] _dependencies;

		public ScriptableObject[] Dependencies => _dependencies;

		public bool IsInitialized => _state == State.Initialized;

		internal State _state = State.None;

		protected void Awake()
		{
			hideFlags |= HideFlags.DontUnloadUnusedAsset;
		}

		internal void Initialize()
		{
			if(_state != State.None)
			{
				return;
			}

			_state = State.Initializing;
			OnSetup();
		}

		internal void Initialized()
		{
			if(_state != State.Initializing)
			{
				return;
			}

			CheckDependencies();
			_state = State.Initialized;
			OnStart();
		}

		internal void Deinitialize()
		{
			if(_state == State.None)
			{
				return;
			}

			OnEnd();
			_state = State.None;
		}

		internal void Dispose()
		{
			try
			{
				Deinitialize();
				OnDispose();
			}
			finally
			{
				_state = State.None;
			}
		}

		/// <summary>
		/// Used to determine if a dependency is valid for use. Automatically checks for <see cref="RaBehaviourSOBase"/>
		/// </summary>
		/// <param name="dependency">The dependency to check</param>
		/// <param name="message">The issue with the dependency</param>
		/// <returns>Return `false` when a dependency is invalid</returns>
		protected virtual bool CheckDependency(ScriptableObject dependency, out string message)
		{
			if(dependency is RaBehaviourSOBase behaviour)
			{
				if(behaviour._state == State.None)
				{
					message = $"Dependency {behaviour.name} not being initialized";
					return false;
				}
			}

			message = default;
			return true;
		}

		protected List<T> GetDependencies<T>(Predicate<T> predicate = null)
		{
			List<T> returnValue = new List<T>();
			for(int i = 0; i < _dependencies.Length; i++)
			{
				var rawDependency = _dependencies[i];
				if(rawDependency is T castedDependency && (predicate == null || predicate(castedDependency)))
				{
					returnValue.Add(castedDependency);
				}
			}
			return returnValue;
		}

		protected T GetDependency<T>(Predicate<T> predicate = null)
		{
			TryGetDependency(out T dependency, predicate);
			return dependency;
		}

		protected bool TryGetDependency<T>(out T dependency, Predicate<T> predicate = null)
		{
			for(int i = 0, c = _dependencies.Length; i < c; i++)
			{
				var rawDependency = _dependencies[i];
				if(rawDependency is T castedDependency && (predicate == null || predicate(castedDependency)))
				{
					dependency = castedDependency;
					return true;
				}
			}

			dependency = default;
			return false;
		}

		protected abstract void OnSetup();
		protected abstract void OnStart();
		protected abstract void OnEnd();

		protected virtual void OnDispose()
		{

		}

		private void CheckDependencies()
		{
			for(int i = 0; i < _dependencies.Length; i++)
			{
				if(!CheckDependency(_dependencies[i], out string message))
				{
					throw new InvalidDependencyException(message);
				}
			}
		}

		internal enum State
		{
			None,
			Initializing,
			Initialized
		}
	}
}