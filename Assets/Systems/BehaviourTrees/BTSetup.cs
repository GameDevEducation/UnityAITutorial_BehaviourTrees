using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BehaviourTree))]
public class BTSetup : MonoBehaviour
{
    public class BlackboardKey : BlackboardKeyBase, System.IEquatable<BlackboardKey>
    {
        public static readonly BlackboardKey CurrentTarget = new BlackboardKey() { Name = "CurrentTarget" };

        public string Name;

        public override bool Equals(object obj)
        {
            return Equals(obj as BlackboardKey);
        }

        public bool Equals(BlackboardKey other)
        {
            return other is not null &&
                   Name == other.Name;
        }

        public override int GetHashCode()
        {
            return System.HashCode.Combine(Name);
        }

        public static bool operator ==(BlackboardKey left, BlackboardKey right)
        {
            return EqualityComparer<BlackboardKey>.Default.Equals(left, right);
        }

        public static bool operator !=(BlackboardKey left, BlackboardKey right)
        {
            return !(left == right);
        }
    }

    [Header("Wander Settings")]
    [SerializeField] float Wander_Range = 10f;

    [Header("Chase Settings")]
    [SerializeField] float Chase_MinAwarenessToChase = 1.5f;
    [SerializeField] float Chase_AwarenessToStopChase = 1f;

    protected BehaviourTree LinkedBT;
    protected CharacterAgent Agent;
    protected AwarenessSystem Sensors;
    protected Blackboard<BlackboardKey> LocalMemory;

    protected float YellCooldown = 0f;

    void Awake()
    {
        Agent = GetComponent<CharacterAgent>();
        LinkedBT = GetComponent<BehaviourTree>();
        Sensors = GetComponent<AwarenessSystem>();
    }

    void Start()
    {
        LocalMemory = BlackboardManager.Instance.GetIndividualBlackboard<BlackboardKey>(this);
        LocalMemory.SetGeneric<DetectableTarget>(BlackboardKey.CurrentTarget, null);

        var BTRoot = LinkedBT.RootNode.Add<BTNode_Selector>("Base Logic");
        BTRoot.AddService<BTServiceBase>("Search for target", (float deltaTime) =>
        {
            // no targets
            if (Sensors.ActiveTargets == null || Sensors.ActiveTargets.Count == 0)
            {
                LocalMemory.SetGeneric<DetectableTarget>(BlackboardKey.CurrentTarget, null);
                return;
            }

            var currentTarget = LocalMemory.GetGeneric<DetectableTarget>(BlackboardKey.CurrentTarget);

            if (currentTarget != null)
            {
                // check if the current is still sensed
                foreach (var candidate in Sensors.ActiveTargets.Values)
                {
                    if (candidate.Detectable == currentTarget &&
                        candidate.Awareness >= Chase_AwarenessToStopChase)
                    {
                        return;
                    }
                }

                // clear our current target
                currentTarget = null;
            }

            // acquire a new target if possible
            float highestAwareness = Chase_MinAwarenessToChase;
            foreach (var candidate in Sensors.ActiveTargets.Values)
            {
                // found a target to acquire
                if (candidate.Awareness >= highestAwareness)
                {
                    currentTarget = candidate.Detectable;
                    highestAwareness = candidate.Awareness;
                }
            }

            LocalMemory.SetGeneric(BlackboardKey.CurrentTarget, currentTarget);
        });

        var chaseRoot = BTRoot.Add<BTNode_Sequence>("Can Logic");
        chaseRoot.AddDecorator<BTDecoratorBase>("Can Chase?", () =>
        {
            var currentTarget = LocalMemory.GetGeneric<DetectableTarget>(BlackboardKey.CurrentTarget);
            return currentTarget != null;
        });

        chaseRoot.Add<BTNode_Action>(new BTNode_Action("Chase Target",
            () =>
            {
                var currentTarget = LocalMemory.GetGeneric<DetectableTarget>(BlackboardKey.CurrentTarget);
                Agent.MoveTo(currentTarget.transform.position);

                return BehaviourTree.ENodeStatus.InProgress;
            },
            () =>
            {
                var currentTarget = LocalMemory.GetGeneric<DetectableTarget>(BlackboardKey.CurrentTarget);
                Agent.MoveTo(currentTarget.transform.position);

                return BehaviourTree.ENodeStatus.InProgress;
            }));

        var wanderRoot = BTRoot.Add<BTNode_Sequence>("Wander").Add<BTNode_Parallel>("Wander Logic") as BTNode_Parallel;

        wanderRoot.SetPrimary(new BTNode_Action("Perform Wander",
            () =>
            {
                Vector3 location = Agent.PickLocationInRange(Wander_Range);

                Agent.MoveTo(location);

                return BehaviourTree.ENodeStatus.InProgress;
            },
            () =>
            {
                return Agent.AtDestination ? BehaviourTree.ENodeStatus.Succeeded : BehaviourTree.ENodeStatus.InProgress;
            }));

        //wanderRoot.SetSecondary(new BTNode_ReturnResult(BehaviourTree.ENodeStatus.InProgress));

        wanderRoot.SetSecondary(new BTNode_Action("Random Yell",
            () =>
            {
                return BehaviourTree.ENodeStatus.InProgress;
            },
            () =>
            {
                YellCooldown -= Time.deltaTime;
                if (YellCooldown <= 0)
                {
                    Debug.Log("I am wandering!");
                    YellCooldown = Random.Range(1.0f, 3.0f);
                }

                return BehaviourTree.ENodeStatus.InProgress;
            }));
    }
}
