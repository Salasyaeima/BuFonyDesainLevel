using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class ZombieAI : MonoBehaviour
{
    public Transform Target;
    private NavMeshAgent z_agent;
    

    void Start()
    {
        z_agent = GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        z_agent.SetDestination(Target.transform.position);
    }

}
