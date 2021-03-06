using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public enum AIState
{
    Idle,
    Sleeping,
    Waking,
    Patrolling,
    ReturningToPatrol,
    InCombat,
    Dead
}

public enum PatrolState
{
    Patrol,
    ReversePatrol,
    Waiting
}

public enum CombatState
{
    Pursuing,
    Strafing,
    StrafingToZone,
    RadialRunToZone,
    MaintainDist,
    ClosingDist,
    BackingUp,
    MovingToZone,
    MovingToAttack,
    Attacking
}

public enum WakeTrigger
{
    Attack,
    Standard
}

public enum AttackingType
{
    Passive,
    Active
}

public enum StrafeDir
{
    Left,
    Right
}

// Enemy AI Script, will likely be reworked to use inheritance once base functionality is polished
public class EnemyAI : MonoBehaviour
{
    private AIManager m_aiManager;
    private AttackZoneManager m_attackZoneManager;

    private NavMeshAgent m_navMeshAgent;
    private AIState m_mainState = AIState.Idle;
    private CombatState m_combatState = CombatState.Strafing;
    private AIState m_stateBeforeHit = AIState.Idle;
    [Header("Movement Values")]
    [SerializeField]
    [Tooltip("The walk speed of the AI")]
    private float m_walkSpeed = 1.5f;
    [SerializeField]
    [Tooltip("The run speed of the AI")]
    private float m_runSpeed = 3.0f;

    // Animation Relevant Variables
    private Animator m_animController;
    private float m_prevAnimSpeed;

    // Patrol Relevant Variables
    [Header("Patrol Values")]
    [SerializeField]
    [Tooltip("Should the AI spawn asleep?")]
    private bool m_spawnAsleep = false;
    [SerializeField]
    [Tooltip("The trigger zone which will wake the AI when the player enters it")]
    private GameObject m_wakeTriggerObj;
    private BoxCollider m_wakeTrigger;
    [SerializeField]
    [Tooltip("The GameObject which holds the position objects for patrolling")]
    private GameObject m_patrolRoute;
    private PatrolState m_patrolState = PatrolState.Patrol;
    private List<Transform> m_patrolRoutePoints = new List<Transform>();
    private Transform m_nextPatrolPoint;
    private Vector3 m_lastPointOnPatrol;
    private float m_patrolTimer = 0.0f;
    private float m_patrolWaitTime = 2.5f;
    private int m_patrolDestinationIndex = 1;
    [SerializeField]
    [Tooltip("The distance the AI will stop from the patrol points")]
    private float m_patrolStoppingDistance = 1.5f;

    // Player/Detection Relevant Variables
    private GameObject m_player;
    private CapsuleCollider m_playerCollider;

    // Combat Relevant Variables
    [Header("Combat Values")]
    [SerializeField]
    [Tooltip("The total health of the AI")]
    private float m_health = 100.0f;
    [SerializeField]
    [Tooltip("The distance from the player that the AI will stop")]
    private float m_playerStoppingDistance = 1.75f;
    //[SerializeField]
    //private bool m_canStrafe = true;
    private float m_delayBeforeStrafe = 0.0f;
    private float m_timeUntilStrafe = 0.0f;
    [SerializeField]
    [Tooltip("The minimum time the AI will stand still in combat before strafing")]
    private float m_minDelayBeforeStrafe = 6.0f;
    [SerializeField]
    [Tooltip("The maximum time the AI will stand still in combat before strafing")]
    private float m_maxDelayBeforeStrafe = 10.0f;
    private StrafeDir m_strafeDir = StrafeDir.Left;
    [SerializeField]
    [Tooltip("The strafing speed of the AI")]
    private float m_strafeSpeed = 1.5f;
    //[SerializeField]
    //private float m_minStrafeRange = 3.0f;
    //[SerializeField]
    //private float m_maxStrafeRange = 5.0f;
    [SerializeField]
    [Tooltip("The distance the AI will check for other obstructing AI during combat")]
    private float m_checkForAIDist = 2.0f;
    [SerializeField]
    [Tooltip("The angles the AI will check for other obstructing AI during combat")]
    private float m_checkForAIAngles = 45.0f;
    [SerializeField]
    [Tooltip("The distance the AI will move away when attempting to avoid other AI")]
    private float m_AIAvoidanceDist = 1.5f;
    private float m_strafeDist;
    private float m_attackTimer;
    [SerializeField]
    [Tooltip("Whether the AI can attack. For debugging")]
    private bool m_attackEnabled = true;
    [SerializeField]
    [Tooltip("The minimum time that has to pass before an actively attacking AI can attack")]
    private float m_minAttackTime = 3.5f;
    [SerializeField]
    [Tooltip("The maximum time that can pass before an actively attacking AI will attack")]
    private float m_maxAttackTime = 7.5f;
    private float m_timeSinceLastAttack = 0.0f;
    [SerializeField]
    [Tooltip("The weapon object which should have a box collider attached for attack collisions")]
    private GameObject m_weapon;
    private BoxCollider m_weaponCollider;
    private Vector3 m_attackZonePos;
    private AttackingType m_currentAttackingType = AttackingType.Passive;
    private AttackZone m_currentAttackZone;
    private AttackZone m_occupiedAttackZone;
    private float m_attackZoneCheckInterval = 5.0f;
    private float m_attackZoneTimer = 0.0f;
    private float m_strafeZoneCheckInterval = 2.0f;
    private float m_strafeZoneTimer = 0.0f;

    // Vision Detection Relevant Variables
    [Header("Player Detection Values")]
    [SerializeField]
    [Tooltip("Whether the AI can detect the player. For debugging")]
    private bool m_playerDetectionEnabled = true;
    [SerializeField]
    [Tooltip("The range that the AI can detect the player")]
    private float m_viewRadius = 7.5f;
    [SerializeField]
    [Range(0.0f, 360.0f)]
    [Tooltip("The angle that the AI can detect the player AKA field of view")]
    private float m_viewAngle = 145.0f;

    [SerializeField]
    [Tooltip("The layer mask for obstacles")]
    private LayerMask m_obstacleMask;
    [SerializeField]
    [Tooltip("The layer mask for AI")]
    private LayerMask m_aiMask;



    //New Input System Shit
    private DeanControls m_inputs;

    private void Awake()
    {

        //Actuall Make the controls
        m_inputs = new DeanControls();


        m_navMeshAgent = GetComponent<NavMeshAgent>();
        m_animController = GetComponent<Animator>();

        m_navMeshAgent.speed = m_walkSpeed;

        SetupPatrolRoutes();

        m_player = GameObject.FindGameObjectWithTag("Player");
        m_playerCollider = m_player.GetComponent<CapsuleCollider>();
        m_weaponCollider = m_weapon.GetComponent<BoxCollider>();

        DisableCollision();

        if (m_spawnAsleep)
        {
            SetAIState(AIState.Sleeping);
        }

        //RandomiseStrafeRange();

        if (m_wakeTriggerObj != null)
        {
            m_wakeTrigger = m_wakeTriggerObj.GetComponent<BoxCollider>();
        }
    }

    private void OnEnable()
    {
        m_inputs.Enable();
    }

    private void Update()
    {
        // Setting the player mat color to see if attack is colliding. Will need removing later on.
        if (m_mainState == AIState.InCombat && m_combatState == CombatState.Attacking)
        {
            m_player.GetComponent<Player>().SetHitVisual(IsAttackCollidingWithPlayer());
        }

        TestingInputs();

        AttackZoneCheck();

        switch (m_mainState)
        {
            // Idle State
            case AIState.Idle:
            {
                if (IsPlayerVisible())
                {
                    // Disabled Detection in Idle for now
                    //SetAIState(AIState.Pursuing);
                }
                break;
            }
            case AIState.Sleeping:
            {
                WakeTriggerCheck();
                break;
            }
            // Patrol Logic
            case AIState.Patrolling:
            {
                if (IsPlayerVisible())
                {
                    SetAIState(AIState.InCombat);
                }
                PatrolUpdate();
                break;
            }
            case AIState.ReturningToPatrol:
            {
                if (HasReachedDestination())
                {
                    SetAIState(AIState.Patrolling);
                }
                break;
            }
            case AIState.InCombat:
            {
                CombatUpdate();
                break;
            }
        }
    }

    private void PatrolUpdate()
    {
        if (m_patrolRoute == null)
        {
            Debug.Log("There is no patrol route attached to the AI. Please attach one.");
            SetAIState(AIState.Idle);
        }

        switch (m_patrolState)
        {
            case PatrolState.Patrol:
            {
                if (HasReachedDestination())
                {
                    if (m_patrolDestinationIndex >= m_patrolRoutePoints.Count - 1)
                    {
                        SetPatrolState(PatrolState.Waiting);
                    }
                    else
                    {
                        m_patrolDestinationIndex++;
                        m_navMeshAgent.destination = m_patrolRoutePoints[m_patrolDestinationIndex].position;
                    }
                }
                break;
            }
            case PatrolState.ReversePatrol:
            {
                if (HasReachedDestination())
                {
                    if (m_patrolDestinationIndex <= 0)
                    {
                        SetPatrolState(PatrolState.Waiting);
                    }
                    else
                    {
                        m_patrolDestinationIndex--;
                        m_navMeshAgent.destination = m_patrolRoutePoints[m_patrolDestinationIndex].position;
                    }
                }
                break;
            }
            case PatrolState.Waiting:
            {
                m_patrolTimer += Time.deltaTime;

                if (m_patrolTimer >= m_patrolWaitTime)
                {
                    if (m_patrolDestinationIndex >= m_patrolRoutePoints.Count - 1)
                    {
                        SetPatrolState(PatrolState.ReversePatrol);
                    }
                    else if (m_patrolDestinationIndex <= 0)
                    {
                        SetPatrolState(PatrolState.Patrol);
                    }
                }
                break;
            }
        }
    }

    private void CombatUpdate()
    {
        // If to help space out attacks a bit more
        if (m_aiManager.CanAttack() && m_combatState != CombatState.Pursuing && m_currentAttackingType == AttackingType.Active)
        {
            m_timeSinceLastAttack += Time.deltaTime;
        }

        //TimedAttackZoneCheck();

        switch (m_combatState)
        {
            // Chase after target/player
            case CombatState.Pursuing:
            {
                m_navMeshAgent.destination = m_player.transform.position;
                //m_navMeshAgent.destination = m_attackZonePos;

                // Very basic detection for reaching destination, will need to be expanded upon
                // i.e. in case of path being blocked
                // Logic from https://answers.unity.com/questions/324589/how-can-i-tell-when-a-navmesh-has-reached-its-dest.html
                if (IsInStrafeRange())
                {
                    AttackZone currentAttackZone = m_attackZoneManager.FindAttackZone(this);

                    if (currentAttackZone != null && currentAttackZone.IsAvailable())
                    {
                        if (m_occupiedAttackZone != null)
                        {
                            m_occupiedAttackZone.EmptyZone();
                        }

                        SetCombatState(CombatState.MaintainDist);
                        SetAttackZone(currentAttackZone);
                        SetOccupiedAttackZone(m_currentAttackZone);
                        m_occupiedAttackZone.SetOccupant(this);
                        //Debug.Log("Destination Reached");
                    }
                    else
                    {
                        SetCombatState(CombatState.RadialRunToZone);
                    }
                }
                break;
            }
            case CombatState.Strafing:
            {
                TimedAttackZoneCheck();

                Strafe();

                StrafeRangeCheck();
                AttackCheck();
                break;
            }
            case CombatState.StrafingToZone:
            {
                Strafe();

                StrafeRangeCheck();

                RadialObstructionCheck();
                StrafeZoneCheck();
                AttackCheck();
                break;
            }
            case CombatState.RadialRunToZone:
            {
                RadialRun();

                StrafeRangeCheck();

                RadialObstructionCheck();
                RadialZoneCheck();
                AttackCheck();
                break;
            }
            case CombatState.MaintainDist:
            {
                TimedAttackZoneCheck();

                transform.LookAt(m_player.transform.position);

                // Todo: Rename StrafeRangeCheck to something more appropriate as it has little to do with strafing
                StrafeRangeCheck();
                TimedBeginStrafeCheck();
                AttackCheck();
                break;
            }
            case CombatState.ClosingDist:
            {
                m_navMeshAgent.destination = m_player.transform.position;

                StrafeRangeCheck();

                // Todo: Optimise this check
                if (Vector3.Distance(m_player.transform.position, transform.position) < m_strafeDist)
                {
                    StrafeOrMaintain();
                }

                // AttackCheck needs to be put here because it was causing a loop higher up
                AttackCheck();

                break;
            }
            case CombatState.BackingUp:
            {
                // Todo: Optimise this check
                if (Vector3.Distance(m_player.transform.position, transform.position) > m_strafeDist)
                {
                    StrafeOrMaintain();
                    return;
                }

                BackUp();
                StrafeRangeCheck();
                transform.LookAt(m_player.transform.position);


                // AttackCheck needs to be put here because it was causing a loop higher up
                AttackCheck();

                break;
            }
            case CombatState.MovingToAttack:
            {
                m_navMeshAgent.destination = m_player.transform.position;

                if (HasReachedDestination())
                {
                    SetCombatState(CombatState.Attacking);
                }
                break;
            }
            case CombatState.MovingToZone:
            {
                if (HasReachedDestination())
                {
                    SetCombatState(CombatState.MaintainDist);
                    Debug.Log("AI: " + name + " reached destination.");
                }
                break;
            }
            case CombatState.Attacking:
            {
                transform.LookAt(m_player.transform.position);
                break;
            }
        }
    }

    private void SetAIState( AIState stateToSet )
    {
        // If changing FROM patrol state, store the last position in the patrol route
        if (m_mainState == AIState.Patrolling)
        {
            m_lastPointOnPatrol = gameObject.transform.position;
        }

        m_mainState = stateToSet;
        ResetAnimTriggers();

        switch (stateToSet)
        {
            case AIState.Idle:
            {
                StartIdleAnim();
                break;
            }
            case AIState.Sleeping:
            {
                SetToPlayDeadAnim();
                break;
            }
            case AIState.Patrolling:
            {
                m_navMeshAgent.destination = m_patrolRoutePoints[m_patrolDestinationIndex].position;
                m_navMeshAgent.stoppingDistance = m_patrolStoppingDistance;

                if (m_patrolState == PatrolState.Patrol || m_patrolState == PatrolState.ReversePatrol)
                {
                    StartWalkAnim();
                }
                else if (m_patrolState == PatrolState.Waiting)
                {
                    StartIdleAnim();
                }
                break;
            }
            case AIState.ReturningToPatrol:
            {
                m_navMeshAgent.stoppingDistance = m_patrolStoppingDistance;
                m_navMeshAgent.autoBraking = false;
                StartWalkAnim();
                break;
            }
            case AIState.InCombat:
            {

                // Registering the enemy as an attacker with the manager
                m_aiManager.RegisterAttacker(this);
                SetCombatState(CombatState.Pursuing);

                break;
            }
            case AIState.Dead:
            {
                StartDeathAnim();
                break;
            }
        }
    }

    private void SetPatrolState( PatrolState stateToSet )
    {
        m_patrolState = stateToSet;
        ResetAnimTriggers();

        switch (stateToSet)
        {
            case PatrolState.Patrol:
            {
                StartWalkAnim();
                break;
            }
            case PatrolState.ReversePatrol:
            {
                StartWalkAnim();
                break;
            }
            case PatrolState.Waiting:
            {
                m_patrolTimer = 0.0f;
                StartIdleAnim();
                break;
            }
        }
    }

    private void SetCombatState( CombatState stateToSet )
    {
        m_combatState = stateToSet;
        ResetAnimTriggers();

        switch (stateToSet)
        {
            case CombatState.Pursuing:
            {
                m_attackZonePos = m_attackZoneManager.RandomiseAttackPosForEnemy(this);

                m_navMeshAgent.stoppingDistance = m_playerStoppingDistance;
                m_navMeshAgent.autoBraking = true;
                RandomiseStrafeRange();
                StartRunAnim();
                break;
            }
            case CombatState.Strafing:
            {
                m_strafeDir = (StrafeDir)Random.Range(0, 2);
                StartStrafeAnim(m_strafeDir);
                break;
            }
            case CombatState.StrafingToZone:
            {
                m_strafeDir = (StrafeDir)Random.Range(0, 2);
                StartStrafeAnim(m_strafeDir);
                break;
            }
            case CombatState.RadialRunToZone:
            {
                m_strafeDir = (StrafeDir)Random.Range(0, 2);
                StartRunAnim();
                break;
            }
            case CombatState.MaintainDist:
            {
                m_timeUntilStrafe = Random.Range(m_minDelayBeforeStrafe, m_maxDelayBeforeStrafe);
                StartCombatIdleAnim();
                break;
            }
            case CombatState.Attacking:
            {
                StartAttackAnim();
                break;
            }
            case CombatState.MovingToZone:
            {
                StartRunAnim();
                break;
            }
            case CombatState.MovingToAttack:
            {
                m_navMeshAgent.destination = m_player.transform.position;
                StartRunAnim();
                break;
            }
            case CombatState.ClosingDist:
            {
                RandomiseStrafeRange();
                StartWalkAnim();
                break;
            }
            case CombatState.BackingUp:
            {
                RandomiseStrafeRange();
                StartWalkBackAnim();
                break;
            }
        }
    }

    private void RadialRun()
    {
        Vector3 offset;
        // Basic start to strafe logic
        if (m_strafeDir == StrafeDir.Left)
        {
            offset = m_player.transform.position - transform.position;
        }
        else
        {
            offset = transform.position - m_player.transform.position;
        }
        Vector3 dir = Vector3.Cross(offset, Vector3.up);
        m_navMeshAgent.SetDestination(transform.position + dir);
    }

    private void Strafe()
    {
        Vector3 offset;
        // Basic start to strafe logic
        if (m_strafeDir == StrafeDir.Left)
        {
            offset = m_player.transform.position - transform.position;
        }
        else
        {
            offset = transform.position - m_player.transform.position;
        }
        Vector3 dir = Vector3.Cross(offset, Vector3.up);
        m_navMeshAgent.SetDestination(transform.position + dir);
        transform.LookAt(m_player.transform.position);
    }

    private void BackUp()
    {
        Vector3 dir = (transform.position - m_player.transform.position).normalized;
        m_navMeshAgent.SetDestination(transform.position + (dir * 2.0f));
        transform.LookAt(m_player.transform.position);
    }

    private void StrafeRangeCheck()
    {
        // This function needs to be renamed as it's not a strafe check
        float distanceToPlayer = Vector3.Distance(transform.position, m_player.transform.position);
        float maxStrafeRange = 0.0f;
        float minStrafeRange = 0.0f;

        if (m_currentAttackingType == AttackingType.Passive)
        {
            maxStrafeRange = m_aiManager.GetPassiveAttackerMaxDist();
            minStrafeRange = m_aiManager.GetActiveAttackerMaxDist();
        }
        else
        {
            maxStrafeRange = m_aiManager.GetActiveAttackerMaxDist();
            minStrafeRange = m_aiManager.GetActiveAttackerMinDist();
        }

        if (distanceToPlayer > maxStrafeRange)
        {
            if (m_occupiedAttackZone != null)
            {
                m_occupiedAttackZone.EmptyZone();
            }
            SetCombatState(CombatState.Pursuing);
        }
        // Player moved closer than strafe range
        if (distanceToPlayer < minStrafeRange && m_combatState != CombatState.BackingUp)
        {
            if (m_occupiedAttackZone != null)
            {
                m_occupiedAttackZone.EmptyZone();
            }
            SetCombatState(CombatState.BackingUp);
            //Debug.Log("Test");
        }
    }

    private void AttackCheck()
    {
        if (m_timeSinceLastAttack >= m_attackTimer && m_aiManager.CanAttack() && m_attackEnabled && m_currentAttackingType == AttackingType.Active)
        {
            SetCombatState(CombatState.MovingToAttack);
            m_aiManager.SetCanAttack(false);
        }
    }

    private void StrafeOrMaintain()
    {
        if (m_currentAttackZone == m_occupiedAttackZone)
        {
            SetCombatState(CombatState.MaintainDist);
        }
        else
        {
            m_combatState = CombatState.RadialRunToZone;
            StartRunAnim();
        }
    }

    private void RandomiseStrafeRange()
    {
        if (m_currentAttackingType == AttackingType.Passive)
        {
            m_strafeDist = Random.Range(m_aiManager.GetActiveAttackerMaxDist(), m_aiManager.GetPassiveAttackerMaxDist()) + m_aiManager.GetActiveAttackerMinDist();
        }
        else
        {
            m_strafeDist = Random.Range(m_aiManager.GetActiveAttackerMinDist(), m_aiManager.GetActiveAttackerMaxDist());
        }
    }

    private bool HasReachedDestination()
    {
        bool destinationReached = false;

        // Just using detection based on distance for now, will need better logic for broken nav paths
        if (!m_navMeshAgent.pathPending)
        {
            if (m_navMeshAgent.remainingDistance < m_navMeshAgent.stoppingDistance)
            {
                destinationReached = true;
            }
        }

        // Very basic detection for reaching destination, will need to be expanded upon
        // i.e. in case of path being blocked
        // Logic from https://answers.unity.com/questions/324589/how-can-i-tell-when-a-navmesh-has-reached-its-dest.html
        //if (!m_navMeshAgent.pathPending)
        //{
        //    if (m_navMeshAgent.remainingDistance <= m_navMeshAgent.stoppingDistance)
        //    {
        //        if (!m_navMeshAgent.hasPath || m_navMeshAgent.velocity.sqrMagnitude == 0f)
        //        {
        //            destinationReached = true;
        //        }
        //    }
        //}

        return destinationReached;
    }

    private bool IsInStrafeRange()
    {
        bool inStrafeRange = false;

        // Just using detection based on distance for now, will need better logic for broken nav paths
        if (m_navMeshAgent.remainingDistance < m_strafeDist)
        {
            inStrafeRange = true;
        }

        return inStrafeRange;
    }

    private void DisableCollision()
    {
        m_weaponCollider.enabled = false;
    }

    private void EnableCollision()
    {
        m_weaponCollider.enabled = true;
    }

    public bool IsAttackCollidingWithPlayer()
    {
        bool isColliding = false;

        // If using this method for actual collision, needs a collider.enabled check
        // But for demonstrating the collision, this is not present currently

        if (m_weaponCollider.bounds.Intersects(m_playerCollider.bounds) && m_weaponCollider.enabled)
        {
            isColliding = true;
            m_weaponCollider.enabled = false;
        }

        return isColliding;
    }

    // DirFromAngle() and IsPlayerVisible() functions use logic from https://www.youtube.com/watch?v=rQG9aUWarwE
    public Vector3 DirFromAngle( float angleInDegrees, bool angleIsGlobal )
    {
        if (!angleIsGlobal)
        {
            angleInDegrees += transform.eulerAngles.y;
        }

        return new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), 0, Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
    }

    // Overloaded DirFromAngle to allow getting the direction from a specified object's position
    public Vector3 DirFromAngle( float angleInDegrees, bool angleIsGlobal, GameObject dirFromObject )
    {
        if (!angleIsGlobal)
        {
            angleInDegrees += dirFromObject.transform.eulerAngles.y;
        }

        return new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), 0, Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
    }

    public bool IsPlayerVisible()
    {
        bool playerIsVisible = false;

        // If in combat, just return true since no point redoing detection
        // Will need changing if de-aggro functionality is implemented
        if (m_mainState == AIState.InCombat)
        {
            return true;
        }

        // Todo: Look into using sqr root distance checks for optimisation
        if (m_playerDetectionEnabled)
        {
            Vector3 distance = transform.position - m_player.transform.position;
            // Checking if player is in range
            if (distance.sqrMagnitude <= m_viewRadius * m_viewRadius)
            {
                // Once player is in range, getting the direction to the player and checking if it's within the AI's FOV
                Vector3 dirToPlayer = (m_player.transform.position - transform.position).normalized;
                if (Vector3.Angle(transform.forward, dirToPlayer) < m_viewAngle * 0.5f)
                {
                    // Once player is in range and in FOV, using Raycast to check if any obstacles are in the way
                    float distanceToPlayer = Vector3.Distance(transform.position, m_player.transform.position);
                    if (!Physics.Raycast(transform.position, dirToPlayer, distanceToPlayer, m_obstacleMask))
                    {
                        playerIsVisible = true;
                    }
                }
            }
        }

        return playerIsVisible;
    }

    private void AttackZoneCheck()
    {
        m_currentAttackZone = m_attackZoneManager.FindAttackZone(this);
    }

    private void TimedAttackZoneCheck()
    {
        m_attackZoneTimer += Time.deltaTime;

        if (m_attackZoneTimer >= m_attackZoneCheckInterval)
        {
            m_attackZoneTimer = 0.0f;

            if (m_currentAttackZone != m_occupiedAttackZone)
            {
                // Todo: Make AttackZone use same enum for attacking types
                if (m_currentAttackZone.GetZoneType() == ZoneType.Active && m_currentAttackingType != AttackingType.Active)
                {
                    SetCombatState(CombatState.BackingUp);
                    return;
                }
                else if (m_currentAttackZone.GetZoneType() == ZoneType.Passive && m_currentAttackingType != AttackingType.Passive)
                {
                    SetCombatState(CombatState.ClosingDist);
                    return;
                }
                if (m_currentAttackZone != null)
                {
                    if (!m_currentAttackZone.IsOccupied())
                    {
                        // Occupy current zone
                        if (m_occupiedAttackZone != null)
                        {
                            m_occupiedAttackZone.EmptyZone();
                        }

                        m_occupiedAttackZone = m_currentAttackZone;
                        m_occupiedAttackZone.SetOccupant(this);
                    }
                    else
                    {
                        // Return to occupied zone
                        //m_navMeshAgent.destination = m_attackZoneManager.RandomiseAttackPosForEnemy(this, m_occupiedAttackZone.GetZoneNum());
                        //SetCombatState(CombatState.MovingToZone);

                        SetCombatState(CombatState.StrafingToZone);
                    }
                }
                else
                {
                    // Return to occupied zone
                    //m_navMeshAgent.destination = m_attackZoneManager.RandomiseAttackPosForEnemy(this, m_occupiedAttackZone.GetZoneNum());
                    //SetCombatState(CombatState.MovingToZone);

                    SetCombatState(CombatState.StrafingToZone);
                }
            }
        }
    }

    private void TimedBeginStrafeCheck()
    {
        m_delayBeforeStrafe += Time.deltaTime;

        if ( m_delayBeforeStrafe > m_timeUntilStrafe )
        {
            SetCombatState(CombatState.StrafingToZone);
            m_delayBeforeStrafe = 0.0f;
        }
    }

    private void StrafeZoneCheck()
    {
        m_strafeZoneTimer += Time.deltaTime;

        if (m_strafeZoneTimer >= m_strafeZoneCheckInterval)
        {
            m_strafeZoneTimer = 0.0f;

            if (m_currentAttackZone != m_occupiedAttackZone)
            {
                if (m_currentAttackZone != null)
                {
                    if (!m_currentAttackZone.IsOccupied())
                    {
                        // Occupy current zone
                        if (m_occupiedAttackZone != null)
                        {
                            m_occupiedAttackZone.EmptyZone();
                        }

                        m_occupiedAttackZone = m_currentAttackZone;
                        m_occupiedAttackZone.SetOccupant(this);
                        SetCombatState(CombatState.MaintainDist);
                    }
                }
            }
        }
    }

    private void RadialObstructionCheck()
    {
        Vector3 dir = transform.forward;
        Vector3 castFrom = transform.position;
        castFrom.y += m_navMeshAgent.height * 0.5f;

        if (m_combatState == CombatState.StrafingToZone)
        {
            if (m_strafeDir == StrafeDir.Left)
            {
                dir = -transform.right;
            }
            else
            {
                dir = transform.right;
            }
        }

        // Three raycasts to check if AI is walking into another AI
        if (Physics.Raycast(castFrom, dir, m_checkForAIDist, m_aiMask) ||
            Physics.Raycast(castFrom, dir + DirFromAngle(-m_checkForAIAngles, false), m_checkForAIDist, m_aiMask) ||
            Physics.Raycast(castFrom, dir + DirFromAngle(m_checkForAIAngles, false), m_checkForAIDist, m_aiMask))
        {
            float currentZoneHalfDist = 0.0f;

            // Finding the distance to compare with the current strafe distance to determine whether the AI should move backwards or forwards
            if (m_currentAttackingType == AttackingType.Passive)
            {
                currentZoneHalfDist = m_aiManager.GetActiveAttackerMaxDist() + ((m_aiManager.GetPassiveAttackerMaxDist() - m_aiManager.GetActiveAttackerMaxDist()) * 0.5f);
            }
            else
            {
                currentZoneHalfDist = m_aiManager.GetActiveAttackerMinDist() + ((m_aiManager.GetActiveAttackerMaxDist() - m_aiManager.GetActiveAttackerMinDist()) * 0.5f);
            }

            if (m_strafeDist > currentZoneHalfDist)
            {
                StartWalkAnim();
                m_strafeDist -= m_AIAvoidanceDist;
                m_combatState = CombatState.ClosingDist;
            }
            else
            {
                StartWalkBackAnim();
                m_strafeDist += m_AIAvoidanceDist;
                m_combatState = CombatState.BackingUp;
            }
        }
    }

    private void RadialZoneCheck()
    {
        if (m_currentAttackZone != m_occupiedAttackZone)
        {
            // Todo: Swap order of if check
            if (m_currentAttackZone != null)
            {
                if (!m_currentAttackZone.IsOccupied())
                {
                    m_combatState = CombatState.StrafingToZone;
                    StartStrafeAnim(m_strafeDir);
                }
            }
        }
    }

    private bool IsInAssignedZone()
    {
        return m_currentAttackZone == m_occupiedAttackZone;
    }

    private void WakeTriggerCheck()
    {
        if (m_wakeTrigger.bounds.Intersects(m_playerCollider.bounds))
        {
            WakeUpAI(WakeTrigger.Standard);
        }
    }

    public void WakeUpAI( WakeTrigger wakeTrigger )
    {
        switch (wakeTrigger)
        {
            case WakeTrigger.Attack:
            {
                SetAIState(AIState.Waking);
                StartStandUpAnim();
                break;
            }
            case WakeTrigger.Standard:
            {
                SetAIState(AIState.Waking);
                StartStandUpAnim();
                break;
            }
        }
    }

    public AIState GetState()
    {
        return m_mainState;
    }

    public CombatState GetCombatState()
    {
        return m_combatState;
    }

    public PatrolState GetPatrolState()
    {
        return m_patrolState;
    }

    public float GetViewRadius()
    {
        return m_viewRadius;
    }

    public float GetViewAngle()
    {
        return m_viewAngle;
    }

    public float GetHealth()
    {
        return m_health;
    }

    public float GetStrafeDist()
    {
        return m_strafeDist;
    }

    public float GetAIAngleCheck()
    {
        return m_checkForAIAngles;
    }

    public float GetAgentHeight()
    {
        return m_navMeshAgent.height;
    }

    private void StartWalkAnim()
    {
        m_navMeshAgent.isStopped = false;
        m_animController.SetTrigger("Walk");
        m_navMeshAgent.speed = m_walkSpeed;
        m_navMeshAgent.updateRotation = true;
    }

    private void StartStrafeAnim( StrafeDir dirToStrafe )
    {
        m_navMeshAgent.isStopped = false;
        m_navMeshAgent.speed = m_strafeSpeed;
        m_navMeshAgent.updateRotation = false;

        switch (dirToStrafe)
        {
            case StrafeDir.Left:
            {
                m_animController.SetTrigger("StrafeLeft");
                break;
            }
            case StrafeDir.Right:
            {
                m_animController.SetTrigger("StrafeRight");
                break;
            }
        }
    }

    private void StartWalkBackAnim()
    {
        m_navMeshAgent.isStopped = false;
        m_animController.SetTrigger("WalkBack");
        m_navMeshAgent.speed = m_walkSpeed;
        m_navMeshAgent.updateRotation = false;
    }

    private void StartRunAnim()
    {
        m_navMeshAgent.isStopped = false;
        m_animController.SetTrigger("Run");
        m_navMeshAgent.speed = m_runSpeed;
        m_navMeshAgent.updateRotation = true;
    }

    private void StartIdleAnim()
    {
        m_navMeshAgent.isStopped = true;
        m_animController.SetTrigger("Idle");
        m_navMeshAgent.updateRotation = true;
    }

    private void StartCombatIdleAnim()
    {
        m_navMeshAgent.isStopped = true;
        m_animController.SetTrigger("CombatIdle");
        m_navMeshAgent.updateRotation = false;
    }

    private void StartAttackAnim()
    {
        m_navMeshAgent.isStopped = true;
        m_animController.SetTrigger("Attack");
        m_navMeshAgent.updateRotation = false;
    }

    private void StartStandUpAnim()
    {
        m_navMeshAgent.isStopped = true;
        m_animController.SetTrigger("StandUp");
        m_navMeshAgent.updateRotation = false;
    }

    private void SetToPlayDeadAnim()
    {
        m_navMeshAgent.isStopped = true;
        m_animController.SetTrigger("LayDown");
        m_navMeshAgent.updateRotation = false;
    }

    private void StartDeathAnim()
    {
        m_navMeshAgent.isStopped = true;
        m_animController.SetTrigger("Death");
        m_navMeshAgent.updateRotation = false;
    }

    private void PlayDamageAnim()
    {
        m_navMeshAgent.isStopped = true;
        m_animController.SetTrigger("TakeHit");
        DisableCollision();
    }

    private void RecoverFromHit()
    {
        // Todo: Refactor this
        if (m_playerDetectionEnabled)
        {
            SetAIState(AIState.InCombat);
        }
        else
        {
            SetAIState(m_stateBeforeHit);
        }
    }

    // Can possibly remove these pause and resume functions, but leave for now
    private void PauseAnimation()
    {
        m_prevAnimSpeed = m_animController.speed;
        m_animController.speed = 0.0f;
    }

    private void ResumeAnimation()
    {
        m_animController.speed = m_prevAnimSpeed;
    }

    private void EndAttack()
    {
        SetCombatState(CombatState.BackingUp);
        m_attackTimer = Random.Range(m_minAttackTime, m_maxAttackTime);
        m_timeSinceLastAttack = 0.0f;

        // Todo: Placeholder logic to demonstrate attacking. Replace with better logic
        m_aiManager.SetCanAttack(true);
    }

    public void TakeDamage( float damageToTake )
    {
        m_stateBeforeHit = m_mainState;

        m_health -= damageToTake;

        if (m_mainState != AIState.Sleeping)
        {
            PlayDamageAnim();
        }

        if (m_health <= 0.0f)
        {
            m_health = 0.0f;
            SetAIState(AIState.Dead);
        }
    }

    public void ChangeStateFromWake()
    {
        if (m_playerDetectionEnabled)
        {
            SetAIState(AIState.InCombat);

            // Had to put this setter here to force path recalculation, otherwise AI would attack immediately.
            m_navMeshAgent.SetDestination(m_player.transform.position);
        }
        else
        {
            SetAIState(AIState.Patrolling);
        }
    }

    private void ResetAnimTriggers()
    {
        m_animController.ResetTrigger("Walk");
        m_animController.ResetTrigger("Idle");
        m_animController.ResetTrigger("Attack");
        m_animController.ResetTrigger("Run");
        m_animController.ResetTrigger("StandUp");
        m_animController.ResetTrigger("LayDown");
        m_animController.ResetTrigger("TakeHit");
        m_animController.ResetTrigger("StrafeLeft");
        m_animController.ResetTrigger("StrafeRight");
        m_animController.ResetTrigger("CombatIdle");
    }

    private void SetupPatrolRoutes()
    {
        // Adding patrol points to a list that the ai can use to follow
        if (m_patrolRoute != null)
        {
            for (int i = 0; i < m_patrolRoute.transform.childCount; i++)
            {
                m_patrolRoutePoints.Add(m_patrolRoute.transform.GetChild(i).gameObject.transform);
            }
        }

        // Checking patrol route points is valid, then setting next patrol point to the second entry
        if (m_patrolRoutePoints.Count >= 2)
        {
            m_nextPatrolPoint = m_patrolRoutePoints[1];
            m_lastPointOnPatrol = m_nextPatrolPoint.position;
        }
    }

    public void SetAIManagerRef( AIManager aiManagerRef )
    {
        m_aiManager = aiManagerRef;
    }

    public void SetAttackZoneManagerRef( AttackZoneManager attackZoneManager )
    {
        m_attackZoneManager = attackZoneManager;
    }

    public AttackingType GetAttackingType()
    {
        return m_currentAttackingType;
    }

    public void SetAttackingType( AttackingType typeToSet )
    {
        m_currentAttackingType = typeToSet;
    }

    public void SetStrafeDist( float distance )
    {
        m_strafeDist = distance;
    }

    public StrafeDir GetStrafeDir()
    {
        return m_strafeDir;
    }

    public float GetAICheckDist()
    {
        return m_checkForAIDist;
    }

    public AttackZone GetAttackZone()
    {
        return m_currentAttackZone;
    }

    public void SetAttackZone( AttackZone zoneToSet )
    {
        m_currentAttackZone = zoneToSet;
    }

    public AttackZone GetOccupiedAttackZone()
    {
        return m_occupiedAttackZone;
    }

    public void SetOccupiedAttackZone( AttackZone zoneToSet )
    {
        m_occupiedAttackZone = zoneToSet;
    }

    private void TestingInputs()
    {
        // Start Patrolling Test Input
        if( m_inputs.Debug.AI_Move.triggered ) 
        {
            SetAIState(AIState.Patrolling);
            if (m_patrolRoute != null)
            {
                m_navMeshAgent.destination = m_patrolRoutePoints[m_patrolDestinationIndex].position;
            }
            //Debug.Log("Going to Next Destination");
        }

        // Start Pursuing Test Input
        if (m_inputs.Debug.AI_Combat.triggered)
        {
            SetAIState(AIState.InCombat);

            //int zoneNum = Random.Range(0, m_aiManager.GetAttackZonesNum());

            //if (m_occupiedAttackZone != null)
            //{
            //  m_occupiedAttackZone.EmptyZone();
            //}
            //m_occupiedAttackZone = m_attackZoneManager.GetAttackZoneByNum(zoneNum, ZoneType.Passive);
            //while (m_occupiedAttackZone.IsOccupied())
            //{
            //    zoneNum = (zoneNum + 1) % m_aiManager.GetAttackZonesNum();
            //    m_occupiedAttackZone = m_attackZoneManager.GetAttackZoneByNum(zoneNum, ZoneType.Passive);
            //}
            ////Debug.Log("AI: " + name + " " + "Zone " + zoneNum);
            //m_occupiedAttackZone.SetOccupant(this);
            //m_navMeshAgent.stoppingDistance = m_patrolStoppingDistance;
            //m_navMeshAgent.destination = m_attackZoneManager.RandomiseAttackPosForEnemy(this, zoneNum);
            ////SetCombatState(CombatState.MovingToZone);

            //m_navMeshAgent.SetDestination(m_attackZoneManager.RandomiseAttackPosForEnemy(this, zoneNum));
            //SetAIState(AIState.InCombat);
        }

        // Start Sleeping Test Input
        //if (Input.GetKeyDown(KeyCode.S))
        //{
        //    SetAIState(AIState.Sleeping);
        //}
    }
}