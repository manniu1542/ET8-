
using ET;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using ZHFSM;
using ZHFSM.Animation;

[EnableClass]
public partial class PlayerHFSM : StateMachineScriptController
{
    private AnimationPlayerManager animationPlayer;
    public InputActionAsset inputPlayer;
    // ReceiveAnimationEventHandler receiveAnimationEventHandler;

    public override void Init()
    {
        var uiInputModule = GameObject.Find("EventSystem").GetComponent<InputSystemUIInputModule>();
        inputPlayer = uiInputModule.actionsAsset;


        animationPlayer = gameObject.GetComponent<AnimationPlayerManager>();
        // receiveAnimationEventHandler = gameObject.GetComponent<ReceiveAnimationEventHandler>();
    }

    //Don't delete or modify the #region & #endregion

    #region Method
	//Service Methods
	[Service("battle_free/BattleFreeUpdate")]
	private void on_BattleFreeUpdate_service(Service service, ServiceExecuteType type)
	{
	
		if (Input.GetKeyDown(KeyCode.LeftShift))
		{
			SetBool("IsRun",true);
		}
		if (Input.GetKeyUp(KeyCode.LeftShift))
		{
			SetBool("IsRun",false);
		}
		if (Input.GetKeyDown(KeyCode.G))
		{
			SetTrigger("tgrRoll");
		}
	}

	[Service("Root/PlayerMoveInput")]
	private void on_PlayerMoveInput_service(Service service, ServiceExecuteType type)
	{
		
	
		Vector2 v = inputPlayer.FindAction("UI/Navigate").ReadValue<Vector2>();
		SetBool("IsWalk", v != Vector2.zero);
		SetBool("IsIdle", v == Vector2.zero);
        
		if (v != Vector2.zero)
		{
			v = v.normalized;
            
			gameObject.transform.forward = Vector3.Lerp(gameObject.transform.forward, new Vector3(v.x, 0, v.y), Time.deltaTime * 10f);
		}
	}

	[Service("Root/CheckHit")]
	private void on_CheckHit_service(Service service, ServiceExecuteType type)
	{
		// if (receiveAnimationEventHandler.beAttack)
		{
			// receiveAnimationEventHandler.beAttack = false;
			// SetTrigger("tgrHit");
			
		}
	}

	[Service("normal/NormalLogicUpdate")]
	private void on_NormalLogicUpdate_service(Service service, ServiceExecuteType type)
	{
	
		if (Input.GetKeyDown(KeyCode.T))
		{
			SetTrigger("tgrTalk");
		}

		if (Input.GetKeyDown(KeyCode.B))
		{
			SetBool("IsBattale",true);
		}

	}

	[Service("Root/NewService0")]
	private void on_NewService0_service(Service service, ServiceExecuteType type)
	{
	}

	//State Methods
	[State("g_hit")]
	private void on_g_hit_execute(State state, StateExecuteType type)
	{
		if (type == StateExecuteType.OnEnter)
		{
			Debug.Log("Walk 切换类型--"+type);
			animationPlayer.RequestTransition("hit");
		
		}
		
	}
	[CanExit("g_hit")]
	private bool can_g_hit_exit(State state)
	{
		return animationPlayer.CurrentFinishPlaying;
	}

	[State("n_walk")]
	private void on_n_walk_execute(State state, StateExecuteType type)
	{
		if (type == StateExecuteType.OnEnter)
		{

			animationPlayer.RequestTransition("walkN");
		
		}
	
	}

	[State("n_idle")]
	private void on_n_idle_execute(State state, StateExecuteType type)
	{
		if (type == StateExecuteType.OnEnter)
		{

			animationPlayer.RequestTransition("idleN");
		
		}
	}

	[State("n_talk")]
	private void on_n_talk_execute(State state, StateExecuteType type)
	{
		
		if (type == StateExecuteType.OnEnter)
		{
	
			animationPlayer.RequestTransition("talkN");
		
		}
		
	}
	[CanExit("n_talk")]
	private bool can_n_talk_exit(State state)
	{
		return animationPlayer.CurrentFinishPlaying;
	}

	[State("free_idle")]
	private void on_free_idle_execute(State state, StateExecuteType type)
	{
		
		if (type == StateExecuteType.OnEnter)
		{
			Debug.Log("free_idle 状态切换--"+type);
			animationPlayer.RequestTransition("free_idle");
		}
	}

	[State("free_walk")]
	private void on_free_walk_execute(State state, StateExecuteType type)
	{
		if (type == StateExecuteType.OnEnter)
		{
			animationPlayer.RequestTransition("free_walk");
		}
	}

	[State("free_run")]
	private void on_free_run_execute(State state, StateExecuteType type)
	{
		if (type == StateExecuteType.OnEnter)
		{
			animationPlayer.RequestTransition("free_run");
		}
	}

	[State("free_roll")]
	private void on_free_roll_execute(State state, StateExecuteType type)
	{
		if (type == StateExecuteType.OnEnter)
		{
			animationPlayer.RequestTransition("free_roll");
		}
	}
	[CanExit("free_roll")]
	private bool can_free_roll_exit(State state)
	{
		return false;
	}

	[State("test")]
	private void on_test_execute(State state, StateExecuteType type)
	{
	}

    #endregion Method
}


