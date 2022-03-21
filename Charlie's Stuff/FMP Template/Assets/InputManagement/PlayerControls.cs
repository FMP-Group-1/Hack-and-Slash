//------------------------------------------------------------------------------
// <auto-generated>
//     This code was auto-generated by com.unity.inputsystem:InputActionCodeGenerator
//     version 1.2.0
//     from Assets/InputManagement/PlayerControls.inputactions
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

public partial class @PlayerControls : IInputActionCollection2, IDisposable
{
    public InputActionAsset asset { get; }
    public @PlayerControls()
    {
        asset = InputActionAsset.FromJson(@"{
    ""name"": ""PlayerControls"",
    ""maps"": [
        {
            ""name"": ""Play"",
            ""id"": ""293d99f3-0121-44ac-9767-de8e4b1ff3fe"",
            ""actions"": [
                {
                    ""name"": ""LightAtatck"",
                    ""type"": ""Button"",
                    ""id"": ""af44bd54-638b-4331-8647-69d6a108e183"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""HeavyAttack"",
                    ""type"": ""Button"",
                    ""id"": ""e6806351-45e6-4f82-ad99-818d2b12d6c6"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""Sheathe/Unsheathe"",
                    ""type"": ""Button"",
                    ""id"": ""1659f43b-8b55-42ec-8be8-4b3085bf93be"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""Whirlwind"",
                    ""type"": ""Button"",
                    ""id"": ""d42f01d8-adb3-4c47-ac3c-11da4aa2feed"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                }
            ],
            ""bindings"": [
                {
                    ""name"": """",
                    ""id"": ""4252cbd6-929a-45ac-ac0e-56d680700331"",
                    ""path"": ""<Mouse>/leftButton"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""LightAtatck"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""377fdbbb-19f0-43dd-8154-3393831c0152"",
                    ""path"": ""<Gamepad>/buttonWest"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""LightAtatck"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""dfd7346e-b7b1-4149-983e-351ff74c832f"",
                    ""path"": ""<Mouse>/rightButton"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""HeavyAttack"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""3c38d507-b1bc-43ee-9e47-cf5b2be3ce55"",
                    ""path"": ""<Gamepad>/buttonNorth"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""HeavyAttack"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""65eed4d9-3efa-43fe-9fcd-4f101edc6641"",
                    ""path"": ""<Keyboard>/q"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Sheathe/Unsheathe"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""c703df55-b2e0-4d61-9eff-e16dddf1020f"",
                    ""path"": ""<Gamepad>/dpad/up"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Sheathe/Unsheathe"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""29202c8d-6eac-432f-bf80-caddc0a541bb"",
                    ""path"": ""<Keyboard>/w"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Whirlwind"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""688a9747-97e7-4b11-b92a-a6e78eb828df"",
                    ""path"": ""<Gamepad>/rightShoulder"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Whirlwind"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                }
            ]
        }
    ],
    ""controlSchemes"": []
}");
        // Play
        m_Play = asset.FindActionMap("Play", throwIfNotFound: true);
        m_Play_LightAtatck = m_Play.FindAction("LightAtatck", throwIfNotFound: true);
        m_Play_HeavyAttack = m_Play.FindAction("HeavyAttack", throwIfNotFound: true);
        m_Play_SheatheUnsheathe = m_Play.FindAction("Sheathe/Unsheathe", throwIfNotFound: true);
        m_Play_Whirlwind = m_Play.FindAction("Whirlwind", throwIfNotFound: true);
    }

    public void Dispose()
    {
        UnityEngine.Object.Destroy(asset);
    }

    public InputBinding? bindingMask
    {
        get => asset.bindingMask;
        set => asset.bindingMask = value;
    }

    public ReadOnlyArray<InputDevice>? devices
    {
        get => asset.devices;
        set => asset.devices = value;
    }

    public ReadOnlyArray<InputControlScheme> controlSchemes => asset.controlSchemes;

    public bool Contains(InputAction action)
    {
        return asset.Contains(action);
    }

    public IEnumerator<InputAction> GetEnumerator()
    {
        return asset.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public void Enable()
    {
        asset.Enable();
    }

    public void Disable()
    {
        asset.Disable();
    }
    public IEnumerable<InputBinding> bindings => asset.bindings;

    public InputAction FindAction(string actionNameOrId, bool throwIfNotFound = false)
    {
        return asset.FindAction(actionNameOrId, throwIfNotFound);
    }
    public int FindBinding(InputBinding bindingMask, out InputAction action)
    {
        return asset.FindBinding(bindingMask, out action);
    }

    // Play
    private readonly InputActionMap m_Play;
    private IPlayActions m_PlayActionsCallbackInterface;
    private readonly InputAction m_Play_LightAtatck;
    private readonly InputAction m_Play_HeavyAttack;
    private readonly InputAction m_Play_SheatheUnsheathe;
    private readonly InputAction m_Play_Whirlwind;
    public struct PlayActions
    {
        private @PlayerControls m_Wrapper;
        public PlayActions(@PlayerControls wrapper) { m_Wrapper = wrapper; }
        public InputAction @LightAtatck => m_Wrapper.m_Play_LightAtatck;
        public InputAction @HeavyAttack => m_Wrapper.m_Play_HeavyAttack;
        public InputAction @SheatheUnsheathe => m_Wrapper.m_Play_SheatheUnsheathe;
        public InputAction @Whirlwind => m_Wrapper.m_Play_Whirlwind;
        public InputActionMap Get() { return m_Wrapper.m_Play; }
        public void Enable() { Get().Enable(); }
        public void Disable() { Get().Disable(); }
        public bool enabled => Get().enabled;
        public static implicit operator InputActionMap(PlayActions set) { return set.Get(); }
        public void SetCallbacks(IPlayActions instance)
        {
            if (m_Wrapper.m_PlayActionsCallbackInterface != null)
            {
                @LightAtatck.started -= m_Wrapper.m_PlayActionsCallbackInterface.OnLightAtatck;
                @LightAtatck.performed -= m_Wrapper.m_PlayActionsCallbackInterface.OnLightAtatck;
                @LightAtatck.canceled -= m_Wrapper.m_PlayActionsCallbackInterface.OnLightAtatck;
                @HeavyAttack.started -= m_Wrapper.m_PlayActionsCallbackInterface.OnHeavyAttack;
                @HeavyAttack.performed -= m_Wrapper.m_PlayActionsCallbackInterface.OnHeavyAttack;
                @HeavyAttack.canceled -= m_Wrapper.m_PlayActionsCallbackInterface.OnHeavyAttack;
                @SheatheUnsheathe.started -= m_Wrapper.m_PlayActionsCallbackInterface.OnSheatheUnsheathe;
                @SheatheUnsheathe.performed -= m_Wrapper.m_PlayActionsCallbackInterface.OnSheatheUnsheathe;
                @SheatheUnsheathe.canceled -= m_Wrapper.m_PlayActionsCallbackInterface.OnSheatheUnsheathe;
                @Whirlwind.started -= m_Wrapper.m_PlayActionsCallbackInterface.OnWhirlwind;
                @Whirlwind.performed -= m_Wrapper.m_PlayActionsCallbackInterface.OnWhirlwind;
                @Whirlwind.canceled -= m_Wrapper.m_PlayActionsCallbackInterface.OnWhirlwind;
            }
            m_Wrapper.m_PlayActionsCallbackInterface = instance;
            if (instance != null)
            {
                @LightAtatck.started += instance.OnLightAtatck;
                @LightAtatck.performed += instance.OnLightAtatck;
                @LightAtatck.canceled += instance.OnLightAtatck;
                @HeavyAttack.started += instance.OnHeavyAttack;
                @HeavyAttack.performed += instance.OnHeavyAttack;
                @HeavyAttack.canceled += instance.OnHeavyAttack;
                @SheatheUnsheathe.started += instance.OnSheatheUnsheathe;
                @SheatheUnsheathe.performed += instance.OnSheatheUnsheathe;
                @SheatheUnsheathe.canceled += instance.OnSheatheUnsheathe;
                @Whirlwind.started += instance.OnWhirlwind;
                @Whirlwind.performed += instance.OnWhirlwind;
                @Whirlwind.canceled += instance.OnWhirlwind;
            }
        }
    }
    public PlayActions @Play => new PlayActions(this);
    public interface IPlayActions
    {
        void OnLightAtatck(InputAction.CallbackContext context);
        void OnHeavyAttack(InputAction.CallbackContext context);
        void OnSheatheUnsheathe(InputAction.CallbackContext context);
        void OnWhirlwind(InputAction.CallbackContext context);
    }
}
