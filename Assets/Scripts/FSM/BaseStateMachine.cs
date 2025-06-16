using System;
using System.Collections.Generic;
using UnityEngine;

public class BaseStateMachine : MonoBehaviour
{
    [SerializeField] private BaseState _initialState;

    public BaseState CurrentState { get; set; }

    // оптимизаци€, сохран€ютс€ в словаре полученные компоненты
    private Dictionary<Type, Component> _cachedComponents;
    public new T GetComponent<T>() where T : Component
    {
        if (_cachedComponents.ContainsKey(typeof(T)))
            return _cachedComponents[typeof(T)] as T;

        var component = base.GetComponent<T>();
        if (component != null)
        {
            _cachedComponents.Add(typeof(T), component);
        }
        return component;
    }

    public Transform currentTarget { get; set; }

    private void Awake()
    {
        CurrentState = _initialState;
        _cachedComponents = new Dictionary<Type, Component>();
    }

    private void Update()
    {
        //CurrentState.Execute(this);
    }
}
