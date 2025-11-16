using InfimaGames.LowPolyShooterPack;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Zombie AI dengan detection range, chase, dan attack
/// Menggunakan State Machine Pattern
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
public class ZombieAI : Health
{
    #region ENUMS
    
    /// <summary>
    /// State zombie AI
    /// </summary>
    public enum ZombieState
    {
        Idle,       // Diam di tempat
        Patrol,     // Jalan-jalan (optional)
        Chase,      // Kejar player
        Attack      // Serang player
    }
    
    #endregion
    
    #region SERIALIZED FIELDS
    
    [Header("References")]
    [Tooltip("Target yang akan dikejar (biasanya player)")]
    [SerializeField]
    private Transform target;
    
    [Header("Detection Settings")]
    [Tooltip("Jarak maksimal untuk mendeteksi player")]
    [SerializeField]
    private float detectionRange = 10f;
    
    [Tooltip("Sudut pandang untuk mendeteksi player (0-180)")]
    [SerializeField]
    private float detectionAngle = 90f;
    
    [Tooltip("Gunakan line of sight check? (cek apakah ada obstacle)")]
    [SerializeField]
    private bool useLineOfSight = true;
    
    [Tooltip("Layer mask untuk line of sight (obstacle)")]
    [SerializeField]
    private LayerMask obstacleMask;
    
    [Header("Chase Settings")]
    [Tooltip("Jarak minimal sebelum berhenti chase (attack range)")]
    [SerializeField]
    private float attackRange = 2f;
    
    [Tooltip("Kecepatan saat chase")]
    [SerializeField]
    private float chaseSpeed = 3.5f;
    
    [Tooltip("Jarak maksimal chase sebelum kembali idle")]
    [SerializeField]
    private float maxChaseDistance = 20f;
    
    [Header("Attack Settings")]
    [Tooltip("Damage per attack")]
    [SerializeField]
    private float attackDamage = 10f;
    
    [Tooltip("Cooldown antara attack")]
    [SerializeField]
    private float attackCooldown = 1.5f;
    
    [Tooltip("Kecepatan saat attack (biasanya lebih lambat)")]
    [SerializeField]
    private float attackSpeed = 1f;
    
    [Header("Patrol Settings (Optional)")]
    [Tooltip("Enable patrol saat idle?")]
    [SerializeField]
    private bool enablePatrol = false;
    
    [Tooltip("Titik-titik patrol")]
    [SerializeField]
    private Transform[] patrolPoints;
    
    [Tooltip("Waktu tunggu di setiap patrol point")]
    [SerializeField]
    private float patrolWaitTime = 2f;
    
    [Header("Debug")]
    [Tooltip("Show detection range di Scene view")]
    [SerializeField]
    private bool showDebugGizmos = true;
    
    #endregion
    
    #region PRIVATE FIELDS
    
    private NavMeshAgent agent;
    private ZombieState currentState = ZombieState.Idle;
    private float lastAttackTime;
    private int currentPatrolIndex = 0;
    private float patrolWaitTimer = 0f;
    private Vector3 startPosition; // Posisi awal untuk kembali
    
    #endregion
    
    #region PROPERTIES
    
    public ZombieState CurrentState => currentState;
    
    #endregion
    
    #region UNITY LIFECYCLE
    
    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        startPosition = transform.position;
        
        // Auto-find player jika target tidak diset
        if (target == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                target = player.transform;
            else
                Debug.LogWarning("ZombieAI: Target not set and Player tag not found!");
        }
        
        // Set initial state
        ChangeState(ZombieState.Idle);
    }
    
    private void Update()
    {
        if (target == null)
            return;
        
        // Update state machine
        UpdateStateMachine();
    }
    
    #endregion
    
    #region STATE MACHINE
    
    private void UpdateStateMachine()
    {
        // Calculate distance to target
        float distanceToTarget = Vector3.Distance(transform.position, target.position);
        
        switch (currentState)
        {
            case ZombieState.Idle:
                UpdateIdleState(distanceToTarget);
                break;
                
            case ZombieState.Patrol:
                UpdatePatrolState(distanceToTarget);
                break;
                
            case ZombieState.Chase:
                UpdateChaseState(distanceToTarget);
                break;
                
            case ZombieState.Attack:
                UpdateAttackState(distanceToTarget);
                break;
        }
    }
    
    private void ChangeState(ZombieState newState)
    {
        // Exit current state
        OnStateExit(currentState);
        
        // Change state
        currentState = newState;
        
        // Enter new state
        OnStateEnter(newState);
        
        Debug.Log($"Zombie {gameObject.name} changed state to: {newState}");
    }
    
    private void OnStateEnter(ZombieState state)
    {
        switch (state)
        {
            case ZombieState.Idle:
                agent.isStopped = true;
                agent.speed = 0f;
                break;
                
            case ZombieState.Patrol:
                agent.isStopped = false;
                agent.speed = chaseSpeed * 0.5f; // Patrol lebih lambat
                if (patrolPoints.Length > 0)
                    agent.SetDestination(patrolPoints[currentPatrolIndex].position);
                break;
                
            case ZombieState.Chase:
                agent.isStopped = false;
                agent.speed = chaseSpeed;
                break;
                
            case ZombieState.Attack:
                agent.isStopped = true;
                agent.speed = attackSpeed;
                break;
        }
    }
    
    private void OnStateExit(ZombieState state)
    {
        // Cleanup jika diperlukan
    }
    
    #endregion
    
    #region STATE UPDATES
    
    private void UpdateIdleState(float distanceToTarget)
    {
        // Cek apakah player dalam detection range
        if (distanceToTarget <= detectionRange)
        {
            // Cek apakah player dalam field of view
            if (IsTargetInFieldOfView())
            {
                // Cek line of sight jika enabled
                if (!useLineOfSight || HasLineOfSight())
                {
                    ChangeState(ZombieState.Chase);
                    return;
                }
            }
        }
        
        // Jika patrol enabled dan tidak ada player terdeteksi
        if (enablePatrol && patrolPoints.Length > 0)
        {
            ChangeState(ZombieState.Patrol);
        }
    }
    
    private void UpdatePatrolState(float distanceToTarget)
    {
        // Cek player detection (priority lebih tinggi dari patrol)
        if (distanceToTarget <= detectionRange)
        {
            if (IsTargetInFieldOfView() && (!useLineOfSight || HasLineOfSight()))
            {
                ChangeState(ZombieState.Chase);
                return;
            }
        }
        
        // Update patrol
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            patrolWaitTimer += Time.deltaTime;
            
            if (patrolWaitTimer >= patrolWaitTime)
            {
                patrolWaitTimer = 0f;
                currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
                agent.SetDestination(patrolPoints[currentPatrolIndex].position);
            }
        }
    }
    
    private void UpdateChaseState(float distanceToTarget)
    {
        // Cek apakah masuk attack range
        if (distanceToTarget <= attackRange)
        {
            ChangeState(ZombieState.Attack);
            return;
        }
        
        // Cek apakah player terlalu jauh (stop chase)
        if (distanceToTarget > maxChaseDistance)
        {
            ChangeState(ZombieState.Idle);
            return;
        }
        
        // Cek apakah kehilangan line of sight
        if (useLineOfSight && !HasLineOfSight())
        {
            // Jika tidak ada line of sight, tetap chase ke last known position
            // Bisa ditambahkan timer untuk kembali idle jika terlalu lama
        }
        
        // Update destination ke player
        agent.SetDestination(target.position);
    }
    
    private void UpdateAttackState(float distanceToTarget)
    {
        // Cek apakah player keluar dari attack range
        if (distanceToTarget > attackRange)
        {
            ChangeState(ZombieState.Chase);
            return;
        }
        
        // Look at target
        Vector3 direction = (target.position - transform.position).normalized;
        direction.y = 0; // Keep rotation on Y axis only
        if (direction != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
        }
        
        // Attack cooldown
        if (Time.time - lastAttackTime >= attackCooldown)
        {
            PerformAttack();
            lastAttackTime = Time.time;
        }
    }
    
    #endregion
    
    #region DETECTION METHODS
    
    /// <summary>
    /// Cek apakah target dalam field of view
    /// </summary>
    private bool IsTargetInFieldOfView()
    {
        Vector3 directionToTarget = (target.position - transform.position).normalized;
        float angleToTarget = Vector3.Angle(transform.forward, directionToTarget);
        
        return angleToTarget <= detectionAngle;
    }
    
    /// <summary>
    /// Cek apakah ada line of sight ke target
    /// </summary>
    private bool HasLineOfSight()
    {
        Vector3 directionToTarget = target.position - transform.position;
        
        // Raycast dari zombie ke player
        if (Physics.Raycast(transform.position + Vector3.up, directionToTarget.normalized, 
            out RaycastHit hit, detectionRange, obstacleMask))
        {
            // Jika hit bukan player, berarti ada obstacle
            return hit.transform == target;
        }
        
        return true; // No obstacle hit
    }
    
    #endregion
    
    #region ATTACK
    
    private void PerformAttack()
    {
        Debug.Log($"Zombie {gameObject.name} attacks {target.name}!");
        
        // Cek apakah target punya Health component
        Health targetHealth = target.GetComponent<Health>();
        if (targetHealth != null)
        {
            targetHealth.TakeDamage(attackDamage, gameObject);
        }
        else
        {
            // Try IDamageable interface
            IDamageable damageable = target.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(attackDamage, gameObject);
            }
        }
        
        // Play attack animation
        // animator.SetTrigger("Attack");
        
        // Play attack sound
        // audioSource.PlayOneShot(attackSound);
    }
    
    #endregion
    
    #region PUBLIC METHODS
    
    /// <summary>
    /// Force zombie to chase specific target
    /// </summary>
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
        if (currentState == ZombieState.Idle || currentState == ZombieState.Patrol)
        {
            ChangeState(ZombieState.Chase);
        }
    }
    
    /// <summary>
    /// Reset zombie to idle state
    /// </summary>
    public void ResetToIdle()
    {
        ChangeState(ZombieState.Idle);
        agent.SetDestination(startPosition);
    }
    
    #endregion
    
    #region DEBUG GIZMOS
    
    private void OnDrawGizmosSelected()
    {
        if (!showDebugGizmos)
            return;
        
        // Detection range (yellow)
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        
        // Attack range (red)
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        
        // Max chase distance (blue)
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, maxChaseDistance);
        
        // Field of view (green)
        Gizmos.color = Color.green;
        Vector3 leftBoundary = Quaternion.Euler(0, -detectionAngle, 0) * transform.forward * detectionRange;
        Vector3 rightBoundary = Quaternion.Euler(0, detectionAngle, 0) * transform.forward * detectionRange;
        Gizmos.DrawLine(transform.position, transform.position + leftBoundary);
        Gizmos.DrawLine(transform.position, transform.position + rightBoundary);
        
        // Line to target (if exists)
        if (target != null)
        {
            Gizmos.color = HasLineOfSight() ? Color.green : Color.red;
            Gizmos.DrawLine(transform.position + Vector3.up, target.position);
        }
        
        // Patrol points
        if (enablePatrol && patrolPoints != null)
        {
            Gizmos.color = Color.cyan;
            for (int i = 0; i < patrolPoints.Length; i++)
            {
                if (patrolPoints[i] != null)
                {
                    Gizmos.DrawWireSphere(patrolPoints[i].position, 0.5f);
                    
                    // Draw line to next patrol point
                    int nextIndex = (i + 1) % patrolPoints.Length;
                    if (patrolPoints[nextIndex] != null)
                    {
                        Gizmos.DrawLine(patrolPoints[i].position, patrolPoints[nextIndex].position);
                    }
                }
            }
        }
    }
    
    #endregion
}