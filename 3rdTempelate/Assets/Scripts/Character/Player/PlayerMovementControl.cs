using Character.Base;
using Config;
using Input;
using Tools;
using UnityEngine;
using UnityEngine.Serialization;

namespace Character.Player
{
    
    public class PlayerMovementControl : CharacterMovementControlBase
    {
        
        private float _rotationAngle;
        private float _angleVolocity;
        [FormerlySerializedAs("_rotationSmoothTime")] [SerializeField] private float rotationSmoothTime;

        private Transform _mainCamera;

        [FormerlySerializedAs("_isLock")] public bool isLock = false;
        //脚步声
        private float _nextStepTime;
        [FormerlySerializedAs("_slowFootTime")] [SerializeField] private float slowFootTime;
        [FormerlySerializedAs("_fastFootTime")] [SerializeField] private float fastFootTime;
        [FormerlySerializedAs("_parryFootTime")] [SerializeField] private float parryFootTime;
        
        [FormerlySerializedAs("_canTurnAndRun")] [SerializeField , Header("是否开启转身跑") , Space(10)] private bool canTurnAndRun;
        
        
        //目标朝向
        private Vector3 _characterTargetDirection;
        public GameObject attackVFXPrefab;
        protected override void Awake()
        {
            currentHealth = maxHealth;
            base.Awake();
            _audioSource = GetComponent<AudioSource>();
            if (_audioSource == null)
                _audioSource = gameObject.AddComponent<AudioSource>();
            _mainCamera = Camera.main.transform;
        }

        protected override void Update()
        {
            base.Update();
            OnLockUpdateXY();
            if (UnityEngine.Input.GetKeyDown(KeyCode.H))
            {
                TakeDamage(10f);
            }

            if (UnityEngine.Input.GetButtonDown("Fire1"))
            {
                Animator.SetTrigger("Attack3N");
            }
           
        }
        public void ATK()
        {
            float attackRadius = 2f;
            Collider[] hitColliders = Physics.OverlapSphere(transform.position, attackRadius);
            Debug.Log("ATK triggered, detected " + hitColliders.Length + " colliders.");
        }
        public void PlayVFX()
        {
            if (attackVFXPrefab != null)
            {
                GameObject vfx = Instantiate(attackVFXPrefab, transform.position, transform.rotation);
                Destroy(vfx, 2f);
            }
            else
            {
                Debug.LogWarning("Attack VFX Prefab is not assigned!");
            }
        }
        public AudioClip footstepSound;
        private AudioSource _audioSource;
        public void FootStep()
        {
            Debug.Log("FootStep event triggered");
            if (footstepSound != null && _audioSource != null)
            {
                _audioSource.PlayOneShot(footstepSound);
            }
        }
        public float maxHealth = 100f;      // 最大血量
        public float currentHealth;         // 当前血量
        public UnityEngine.UI.Slider healthSlider; // 血条UI引用
        public void TakeDamage(float damage)
        {
            currentHealth -= damage;
            if (currentHealth < 0) currentHealth = 0;
            UpdateHealthUI();
        }
        private void UpdateHealthUI()
        {
            if (healthSlider != null)
                healthSlider.value = currentHealth / maxHealth;
        }
        private void LateUpdate()
        {

            CharacterRotationControl();


            UpdateAnimator();
        }


        private void CharacterRotationControl()
        {
            
            if(!CharacterIsOnGround || isLock) return;
            
            if (Animator.GetBool(AnimationID.HasInput))
            {
                _rotationAngle = 
                    Mathf.Atan2(GameInputManager.MainInstance.Movement.x , GameInputManager.MainInstance.Movement.y) * 
                    Mathf.Rad2Deg + _mainCamera.eulerAngles.y;
                
            }
            
            
            if (Animator.GetBool(AnimationID.HasInput) && Animator.AnimationAtTag("Motion"))
            {

                if(canTurnAndRun)
                    Animator.SetFloat(AnimationID.DeltaAngle , DevelopmentTools.GetDeltaAngle(transform, _characterTargetDirection.normalized));
                if (canTurnAndRun)
                {
                    if(Animator.GetFloat(AnimationID.DeltaAngle) < -135f && Animator.GetBool(AnimationID.Run)) return;
                    if(Animator.GetFloat(AnimationID.DeltaAngle) > 135f && Animator.GetBool(AnimationID.Run)) return;
                }
                
                transform.eulerAngles = Vector3.up * Mathf.SmoothDampAngle(transform.eulerAngles.y, _rotationAngle,
                    ref _angleVolocity, rotationSmoothTime);
            
                if (canTurnAndRun)
                {
                    //得到我们要转到的目标方向
                    _characterTargetDirection = Quaternion.Euler(0, _rotationAngle, 0) * Vector3.forward;
                   
                }
                
               
            }
            // if(_canTurnAndRun)
            //     _animator.SetFloat(AnimationID.DeltaAngle , DevelopmentToos.GetDeltaAngle(transform, _characterTargetDirection.normalized));
            
        }
        
       
        

        private void UpdateAnimator()
        {
            if(!CharacterIsOnGround) return;
            if (CharacterIsOnGround && Animator.GetBool(AnimationID.HasInput))
            {
                _nextStepTime -= Time.deltaTime;
                if (_nextStepTime <= 0f)
                {
                    float stepInterval = Animator.GetBool(AnimationID.Run) ? fastFootTime : slowFootTime;
                    if (stepInterval > 0f)
                    {
                        FootStep();
                        _nextStepTime = stepInterval;
                    }
                }
            }
            else
            {
                _nextStepTime = 0f;
            }
            Animator.SetBool(AnimationID.HasInput, GameInputManager.MainInstance.Movement != Vector2.zero);
            
            if (Animator.GetBool(AnimationID.HasInput))
            {
                
                Animator.SetBool(AnimationID.Run , GameInputManager.MainInstance.Run);

                
                Animator.SetFloat(AnimationID.Movement , (Animator.GetBool(AnimationID.Run) ? 2f : GameInputManager.MainInstance.Movement.sqrMagnitude ), 0.25f, Time.deltaTime);
            }
            else
            {
                Animator.SetFloat(AnimationID.Movement , 0f, 0.25f, Time.deltaTime);
                if (Animator.GetFloat(AnimationID.Movement) < 0.2f)
                {
                    Animator.SetBool(AnimationID.Run , false);

                }
            }
        }

        private void OnLockUpdateXY()
        {
            if (isLock)
            {
                Animator.SetFloat(AnimationID.Horizontal, GameInputManager.MainInstance.Movement.x , 0.25f, Time.deltaTime);
                Animator.SetFloat(AnimationID.Vertical, GameInputManager.MainInstance.Movement.y , 0.25f, Time.deltaTime);
            }
        }
        
    }

}