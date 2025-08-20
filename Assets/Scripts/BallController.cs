using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(Rigidbody))]
public class BallController : MonoBehaviour
{
    [SerializeField] private float hapticDuration = 0.1f;
    [SerializeField] private float maxHapticIntensity = 0.1f;
    [SerializeField] private GameObject mazeGenerator;

    public AudioClip rollingSound;
    public AudioClip bounceSound;
    public AudioClip collectSound;
    public AudioClip spikesSound;
    public AudioClip victorySound;
    private readonly HashSet<Collider> _activeCollisions = new();

    private bool _isRolling;

    private XRBaseController _leftController;

    private AudioSource _localAudio;
    private Rigidbody _rb;
    private XRBaseController _rightController;
    private AudioSource _soundAudioSource;

    private void Awake()
    {
        _soundAudioSource = GameObject.FindWithTag("SoundPlayer").GetComponent<AudioSource>();
        _localAudio = GetComponent<AudioSource>();
        _rb = GetComponent<Rigidbody>();
        _isRolling = false;
    }

    private void Update()
    {
        HandleRollingSound();

        if (!_leftController)
            _leftController = GameObject.FindWithTag("LeftController").GetComponent<XRBaseController>();
        if (!_rightController)
            _rightController = GameObject.FindWithTag("RightController").GetComponent<XRBaseController>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (_activeCollisions.Add(collision.collider))
        {
            var impactForce = collision.relativeVelocity.magnitude;
            PlayBounceSound(impactForce);
            TriggerHapticFeedback(impactForce);
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        _activeCollisions.Remove(collision.collider);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Key"))
        {
            var door = GameObject.FindWithTag("Door");
            if (door != null) door.SetActive(false);

            other.gameObject.SetActive(false);
            PlayCollectSound();
        }
        else if (other.CompareTag("Spikes"))
        {
            PlaySpikesSound();
            mazeGenerator.GetComponent<MazeDisplay>().AddSpikesTime();
        }
        else if (other.CompareTag("End"))
        {
            EndReached();
            PlayVictorySound();
        }
    }

    private void HandleRollingSound()
    {
        if (_rb.velocity.magnitude > 0.1f && IsGrounded())
        {
            if (!_isRolling)
            {
                _localAudio.clip = rollingSound;
                _localAudio.loop = true;
                _localAudio.Play();
                _isRolling = true;
            }

            _localAudio.pitch = Mathf.Clamp(_rb.velocity.magnitude / 5.0f, 1.0f, 2.0f); // Scale pitch based on speed
        }
        else
        {
            if (_isRolling)
            {
                _localAudio.Stop();
                _isRolling = false;
            }
        }
    }

    private void TriggerHapticFeedback(float impactForce)
    {
        var intensity = Mathf.Clamp(impactForce / 20f, 0.01f, maxHapticIntensity);

        if (_leftController) _leftController.SendHapticImpulse(intensity, hapticDuration);
        if (_rightController) _rightController.SendHapticImpulse(intensity, hapticDuration);
    }

    private bool IsGrounded()
    {
        return Physics.Raycast(transform.position, Vector3.down, out _, 0.6f);
    }

    private void PlayBounceSound(float impactForce)
    {
        _localAudio.PlayOneShot(bounceSound, Mathf.Clamp(impactForce, 0.1f, 1.5f));
    }

    private void PlayCollectSound()
    {
        _soundAudioSource.PlayOneShot(collectSound, 1f);
    }

    private void PlaySpikesSound()
    {
        _soundAudioSource.PlayOneShot(spikesSound, 0.3f);
    }

    private void PlayVictorySound()
    {
        _soundAudioSource.PlayOneShot(victorySound, 0.7f);
    }

    private void EndReached()
    {
        GameEvents.EndReached();
    }
}