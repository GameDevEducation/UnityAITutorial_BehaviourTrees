using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;

[RequireComponent(typeof(BehaviourTree))]
public class BTSetup : MonoBehaviour
{
    [Header("Wander Settings")]
    [SerializeField] float Wander_Range = 10f;

    [Header("Chase Settings")]
    [SerializeField] float Chase_MinAwarenessToChase = 1.5f;
    [SerializeField] float Chase_AwarenessToStopChase = 1f;

    DetectableTarget Chase_CurrentTarget;

    protected BehaviourTree LinkedBT;
    protected CharacterAgent Agent;
    protected AwarenessSystem Sensors;

    void Awake()
    {
        Agent = GetComponent<CharacterAgent>();
        LinkedBT = GetComponent<BehaviourTree>();
        Sensors = GetComponent<AwarenessSystem>();

        var BTRoot = LinkedBT.RootNode.Add<BTNode_Selector>("Base Logic");

        var chaseRoot = BTRoot.Add(new BTNode_Condition("Can Chase",
            () =>
            {
                // no targets
                if (Sensors.ActiveTargets == null || Sensors.ActiveTargets.Count == 0)
                    return false;

                if (Chase_CurrentTarget != null)
                {
                    // check if the current is still sensed
                    foreach (var candidate in Sensors.ActiveTargets.Values)
                    {
                        if (candidate.Detectable == Chase_CurrentTarget &&
                            candidate.Awareness >= Chase_AwarenessToStopChase)
                        {
                            return true;
                        }
                    }

                    // clear our current target
                    Chase_CurrentTarget = null;
                }

                // acquire a new target if possible
                float highestAwareness = Chase_MinAwarenessToChase;
                foreach (var candidate in Sensors.ActiveTargets.Values)
                {
                    // found a target to acquire
                    if (candidate.Awareness >= highestAwareness)
                    {
                        Chase_CurrentTarget = candidate.Detectable;
                        highestAwareness = candidate.Awareness;
                    }
                }

                return Chase_CurrentTarget != null;
            }));
        chaseRoot.Add<BTNode_Action>("Chase Target",
            () =>
            {
                Agent.MoveTo(Chase_CurrentTarget.transform.position);

                return BehaviourTree.ENodeStatus.InProgress;
            },
            () =>
            {
                Agent.MoveTo(Chase_CurrentTarget.transform.position);

                return BehaviourTree.ENodeStatus.InProgress;
            });

        var wanderRoot = BTRoot.Add<BTNode_Sequence>("Wander");
        wanderRoot.Add<BTNode_Action>("Perform Wander",
            () =>
            {
                Vector3 location = Agent.PickLocationInRange(Wander_Range);

                Agent.MoveTo(location);

                return BehaviourTree.ENodeStatus.InProgress;
            },
            () =>
            {
                return Agent.AtDestination ? BehaviourTree.ENodeStatus.Succeeded : BehaviourTree.ENodeStatus.InProgress;
            });
    }
}
