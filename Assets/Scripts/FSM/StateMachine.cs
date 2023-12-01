using System.Collections.Generic;

public class StateMachine<T>
{
    public T currentState;
    private Dictionary<T, List<StateTransition<T>>> stateTransitions = new Dictionary<T, List<StateTransition<T>>>();
    private Dictionary<T, System.Action> stateActions = new Dictionary<T, System.Action>();

    public StateMachine(T initialState)
    {
        currentState = initialState;
    }

    public void FirstState()
    {
        ExecuteStateAction(currentState);
    }

    public void AddTransition(T fromState, T toStateTrue, T toStateFalse, System.Func<bool> condition)
    {
        if (!stateTransitions.ContainsKey(fromState))
            stateTransitions[fromState] = new List<StateTransition<T>>();

        stateTransitions[fromState].Add(new StateTransition<T>(toStateTrue, condition));
        stateTransitions[fromState].Add(new StateTransition<T>(toStateFalse, () => !condition()));
    }

    public void SetStateAction(T state, System.Action action)
    {
        stateActions[state] = action;
    }

    public void Update()
    {
        if (stateTransitions.ContainsKey(currentState))
        {
            foreach (var transition in stateTransitions[currentState])
            {
                if (transition.Condition())
                {
                    currentState = transition.NextState;
                    ExecuteStateAction(currentState);
                    break;
                }
            }
        }
    }

    private void ExecuteStateAction(T state)
    {
        if (stateActions.ContainsKey(state))
        {
            stateActions[state].Invoke();
           
        }
    }

    private class StateTransition<U>
    {
        public U NextState { get; private set; }
        public System.Func<bool> Condition { get; private set; }

        public StateTransition(U nextState, System.Func<bool> condition)
        {
            NextState = nextState;
            Condition = condition;
        }
    }
}
