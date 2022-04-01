using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AIDebugDisplay : MonoBehaviour
{
    [SerializeField]
    private EnemyAI m_aiDebugTarget;

    private Text m_aiNameText;
    private Text m_aiStateText;
    private Text m_aiSubstateText;
    private Text m_aiHealth;
    private Text m_playerDetectedText;
    private Text m_strafeAtDistText;
    void Awake()
    {
        SetupDebugDisplay();
    }

    void Update()
    {
        DebugTextUpdate();
    }

    private void SetupDebugDisplay()
    {
        m_aiNameText = GameObject.Find("AINameText").GetComponent<Text>();
        m_aiStateText = GameObject.Find("AIStateText").GetComponent<Text>();
        m_aiSubstateText = GameObject.Find("AISubstateText").GetComponent<Text>();
        m_aiHealth = GameObject.Find("AIHealthText").GetComponent<Text>();
        m_playerDetectedText = GameObject.Find("PlayerDetectText").GetComponent<Text>();
        m_strafeAtDistText = GameObject.Find("StrafeDistText").GetComponent<Text>();

        m_aiNameText.text = "AI: " + m_aiDebugTarget.name;
    }

    private void DebugTextUpdate()
    {
        AIState currentAIState = m_aiDebugTarget.GetState();

        m_aiStateText.text = "AI State: " + currentAIState;
        m_aiHealth.text = "AI Health: " + m_aiDebugTarget.GetHealth();
        m_playerDetectedText.text = "Player Detected: " + m_aiDebugTarget.IsPlayerVisible();
        m_strafeAtDistText.text = "Strafe Distance: " + m_aiDebugTarget.GetStrafeDist();

        if (currentAIState == AIState.InCombat || currentAIState == AIState.Patrolling)
        {
            if (currentAIState == AIState.InCombat)
            {
                m_aiSubstateText.text = "Combat State: " + m_aiDebugTarget.GetCombatState();
            }
            else if (currentAIState == AIState.Patrolling)
            {
                m_aiSubstateText.text = "Patrol State: " + m_aiDebugTarget.GetPatrolState();
            }
        }
        else
        {
            m_aiSubstateText.text = "Substate: N/A";
        }
    }
}
