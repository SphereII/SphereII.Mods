using System.Collections;
using UnityEngine;

// Credit: https://assetstore.unity.com/packages/vfx/particles/particle-attractor-86896

[RequireComponent(typeof(ParticleSystem))]
public class particleAttractorLinear : MonoBehaviour
{
    ParticleSystem ps;
    ParticleSystem.Particle[] m_Particles;
    public Transform target;
    public float speed = 5f;
    int numParticlesAlive;
    void Start()
    {
        ps = GetComponent<ParticleSystem>();
        if (!GetComponent<Transform>())
        {
            GetComponent<Transform>();
        }
        
    }

    void Update()
    {
        if (target == null) return;
        
        m_Particles = new ParticleSystem.Particle[ps.main.maxParticles];
        numParticlesAlive = ps.GetParticles(m_Particles);
        float step = speed * Time.deltaTime;
        for (int i = 0; i < numParticlesAlive; i++)
        {
            m_Particles[i].position = Vector3.LerpUnclamped(m_Particles[i].position, target.position, step);
        }

        ps.SetParticles(m_Particles, numParticlesAlive);
    }
}