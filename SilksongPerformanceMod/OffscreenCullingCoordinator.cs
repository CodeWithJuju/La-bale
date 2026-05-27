using System;
using System.Collections.Generic;
using UnityEngine;

namespace SilksongPerformanceMod;

internal sealed class OffscreenCullingCoordinator : MonoBehaviour
{
    private static readonly string[] RiskyNameParts =
    {
        "Hero",
        "Health",
        "Enemy",
        "Damage",
        "Hit",
        "Attack",
        "Nail",
        "Projectile",
        "Bullet",
        "Collider",
        "Trigger",
        "Spawner",
        "Spawn",
        "Platform",
        "Lift",
        "Door",
        "Switch",
        "Interact",
        "Collect",
        "Pickup",
        "Item",
        "NPC",
        "Boss",
        "Alert",
        "Sight",
        "Walker",
        "Turret",
        "Corpse",
        "Save",
        "Bench",
        "Transition",
        "Scene",
        "Manager",
        "Controller",
        "Input",
        "Map",
        "HUD",
        "UI",
        "Canvas",
        "Audio",
        "WaterPhysics",
        "Animator"
    };

    private static OffscreenCullingCoordinator? _instance;

    private readonly List<ParticleCandidate> _particleCandidates = new();

    private readonly List<RendererCandidate> _rendererCandidates = new();

    private readonly List<BehaviourCandidate> _behaviourCandidates = new();

    private readonly HashSet<int> _particleCandidateIds = new();

    private readonly HashSet<int> _rendererCandidateIds = new();

    private readonly HashSet<int> _behaviourCandidateIds = new();

    private readonly HashSet<int> _rejectedCandidateIds = new();

    private int _nextScanFrame;

    private int _particleIndex;

    private int _rendererIndex;

    private int _behaviourIndex;

    private bool _restoredWhileDisabled = true;

    public static void Install()
    {
        if (_instance != null)
        {
            return;
        }

        GameObject gameObject = new("Performance Offscreen Culling");
        DontDestroyOnLoad(gameObject);
        _instance = gameObject.AddComponent<OffscreenCullingCoordinator>();
    }

    public static void Uninstall()
    {
        if (_instance == null)
        {
            return;
        }

        OffscreenCullingCoordinator instance = _instance;
        _instance = null;
        instance.RestoreAll();
        Destroy(instance.gameObject);
    }

    private void OnDestroy()
    {
        RestoreAll();
        if (_instance == this)
        {
            _instance = null;
        }
    }

    private void Update()
    {
        if (!Application.isPlaying ||
            !ModSettings.EnableAggressiveOffscreenCulling.Value ||
            !RuntimeContext.ShouldApplyDistanceThrottle())
        {
            RestoreAllOnceWhileDisabled();
            return;
        }

        _restoredWhileDisabled = false;

        int frame = Time.frameCount;
        if (frame >= _nextScanFrame)
        {
            ScanCandidates();
            _nextScanFrame = frame + Math.Max(120, ModSettings.OffscreenCullingScanIntervalFrames.Value);
        }

        Camera? camera = GetMainCamera();
        if (camera == null)
        {
            RestoreAllOnceWhileDisabled();
            return;
        }

        int budget = Math.Max(20, ModSettings.OffscreenCullingMaxCandidatesPerFrame.Value);
        if (ModSettings.EnableOffscreenLoopingParticleSuspend.Value)
        {
            ProcessParticleCandidates(camera, budget);
        }
        else
        {
            RestoreParticles();
        }

        if (ModSettings.EnableOffscreenVisualRendererDisable.Value)
        {
            ProcessRendererCandidates(camera, budget);
        }
        else
        {
            RestoreRenderers();
        }

        if (ModSettings.EnableOffscreenDecorativeBehaviourDisable.Value)
        {
            ProcessBehaviourCandidates(camera, budget);
        }
        else
        {
            RestoreBehaviours();
        }
    }

    private void ScanCandidates()
    {
        PruneDestroyedCandidates();

        if (ModSettings.EnableOffscreenLoopingParticleSuspend.Value)
        {
            foreach (ParticleSystem particleSystem in FindActiveObjects<ParticleSystem>())
            {
                TryAddParticleCandidate(particleSystem);
            }
        }

        if (ModSettings.EnableOffscreenDecorativeBehaviourDisable.Value)
        {
            AddBehaviourCandidates(FindActiveObjects<AmbientFloat>());
            AddBehaviourCandidates(FindActiveObjects<AmbientSway>());
            AddBehaviourCandidates(FindActiveObjects<ColourDistanceSilhouette>());
            AddBehaviourCandidates(FindActiveObjects<FloatingObject>());
            AddBehaviourCandidates(FindActiveObjects<JitterSelfSimple>());
            AddBehaviourCandidates(FindActiveObjects<LoopRotator>());
            AddBehaviourCandidates(FindActiveObjects<SpriteFadePulse>());
            AddBehaviourCandidates(FindActiveObjects<TK2DSpriteFadePulse>());
        }
    }

    private static T[] FindActiveObjects<T>()
        where T : UnityEngine.Object
    {
        return UnityEngine.Object.FindObjectsByType<T>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
    }

    private void TryAddParticleCandidate(ParticleSystem particleSystem)
    {
        if (particleSystem == null)
        {
            return;
        }

        int id = particleSystem.GetInstanceID();
        if (_particleCandidateIds.Contains(id) || _rejectedCandidateIds.Contains(id))
        {
            return;
        }

        ParticleSystem.MainModule main = particleSystem.main;
        if (!main.loop || !IsSafeVisualCandidate(particleSystem.gameObject))
        {
            _rejectedCandidateIds.Add(id);
            return;
        }

        Renderer? renderer = particleSystem.GetComponent<Renderer>();
        Bounds bounds = CreateFallbackBounds(particleSystem.transform);
        if (renderer != null && TryReadBounds(renderer, particleSystem.transform, out Bounds rendererBounds))
        {
            bounds = rendererBounds;
        }

        _particleCandidateIds.Add(id);
        _particleCandidates.Add(new ParticleCandidate(id, particleSystem, renderer, bounds));
    }

    private void TryAddRendererCandidate(Renderer renderer, bool knownSafe)
    {
        if (renderer == null || !renderer.enabled || renderer is ParticleSystemRenderer)
        {
            return;
        }

        int id = renderer.GetInstanceID();
        if (_rendererCandidateIds.Contains(id) || _rejectedCandidateIds.Contains(id))
        {
            return;
        }

        string rendererTypeName = renderer.GetType().Name;
        if (rendererTypeName.Contains("Trail") || rendererTypeName.Contains("Line"))
        {
            _rejectedCandidateIds.Add(id);
            return;
        }

        if (!knownSafe && !IsSafeVisualCandidate(renderer.gameObject))
        {
            _rejectedCandidateIds.Add(id);
            return;
        }

        Bounds bounds = CreateFallbackBounds(renderer.transform);
        if (TryReadBounds(renderer, renderer.transform, out Bounds rendererBounds))
        {
            bounds = rendererBounds;
        }

        _rendererCandidateIds.Add(id);
        _rendererCandidates.Add(new RendererCandidate(id, renderer, bounds));
    }

    private void AddBehaviourCandidates<T>(T[] behaviours)
        where T : Behaviour
    {
        foreach (T behaviour in behaviours)
        {
            TryAddBehaviourCandidate(behaviour);
        }
    }

    private void TryAddBehaviourCandidate(Behaviour behaviour)
    {
        if (behaviour == null || !behaviour.enabled)
        {
            return;
        }

        int id = behaviour.GetInstanceID();
        if (_behaviourCandidateIds.Contains(id) || _rejectedCandidateIds.Contains(id))
        {
            return;
        }

        if (!IsSafeVisualCandidate(behaviour.gameObject))
        {
            _rejectedCandidateIds.Add(id);
            return;
        }

        Renderer? renderer = behaviour.GetComponent<Renderer>();
        if (renderer == null)
        {
            renderer = behaviour.GetComponentInChildren<Renderer>(includeInactive: false);
        }

        if (renderer == null || renderer is ParticleSystemRenderer)
        {
            _rejectedCandidateIds.Add(id);
            return;
        }

        Bounds bounds = CreateFallbackBounds(behaviour.transform);
        if (TryReadBounds(renderer, behaviour.transform, out Bounds rendererBounds))
        {
            bounds = rendererBounds;
        }

        _behaviourCandidateIds.Add(id);
        _behaviourCandidates.Add(new BehaviourCandidate(id, behaviour, renderer, bounds));

        if (ModSettings.EnableOffscreenVisualRendererDisable.Value)
        {
            TryAddRendererCandidate(renderer, knownSafe: true);
        }
    }

    private void ProcessParticleCandidates(Camera camera, int budget)
    {
        if (_particleCandidates.Count == 0)
        {
            return;
        }

        int count = Math.Min(budget, _particleCandidates.Count);
        for (int i = 0; i < count; i++)
        {
            if (_particleIndex >= _particleCandidates.Count)
            {
                _particleIndex = 0;
            }

            ParticleCandidate candidate = _particleCandidates[_particleIndex++];
            if (candidate.ParticleSystem == null)
            {
                continue;
            }

            Bounds bounds = GetCandidateBounds(candidate.Renderer, candidate.ParticleSystem.transform, candidate.LastBounds, candidate.Suspended);
            bool shouldRun = IsVisibleOrProtected(bounds, candidate.ParticleSystem.transform, camera);
            if (shouldRun)
            {
                ResumeParticle(candidate);
            }
            else
            {
                SuspendParticle(candidate);
            }
        }
    }

    private void ProcessRendererCandidates(Camera camera, int budget)
    {
        if (_rendererCandidates.Count == 0)
        {
            return;
        }

        int count = Math.Min(budget, _rendererCandidates.Count);
        for (int i = 0; i < count; i++)
        {
            if (_rendererIndex >= _rendererCandidates.Count)
            {
                _rendererIndex = 0;
            }

            RendererCandidate candidate = _rendererCandidates[_rendererIndex++];
            if (candidate.Renderer == null)
            {
                continue;
            }

            Bounds bounds = GetCandidateBounds(candidate.Renderer, candidate.Renderer.transform, candidate.LastBounds, candidate.Suspended);
            bool shouldRun = IsVisibleOrProtected(bounds, candidate.Renderer.transform, camera);
            if (shouldRun)
            {
                ResumeRenderer(candidate);
            }
            else
            {
                SuspendRenderer(candidate);
            }
        }
    }

    private void ProcessBehaviourCandidates(Camera camera, int budget)
    {
        if (_behaviourCandidates.Count == 0)
        {
            return;
        }

        int count = Math.Min(budget, _behaviourCandidates.Count);
        for (int i = 0; i < count; i++)
        {
            if (_behaviourIndex >= _behaviourCandidates.Count)
            {
                _behaviourIndex = 0;
            }

            BehaviourCandidate candidate = _behaviourCandidates[_behaviourIndex++];
            if (candidate.Behaviour == null)
            {
                continue;
            }

            Bounds bounds = GetCandidateBounds(candidate.AnchorRenderer, candidate.Behaviour.transform, candidate.LastBounds, candidate.Suspended);
            bool shouldRun = IsVisibleOrProtected(bounds, candidate.Behaviour.transform, camera);
            if (shouldRun)
            {
                ResumeBehaviour(candidate);
            }
            else
            {
                SuspendBehaviour(candidate);
            }
        }
    }

    private static void SuspendParticle(ParticleCandidate candidate)
    {
        if (candidate.Suspended || candidate.ParticleSystem == null || !candidate.ParticleSystem.gameObject.activeInHierarchy)
        {
            return;
        }

        candidate.WasPlaying = candidate.ParticleSystem.isPlaying || candidate.ParticleSystem.isEmitting;
        ParticleSystem.EmissionModule emission = candidate.ParticleSystem.emission;
        candidate.WasEmissionEnabled = emission.enabled;

        if (!candidate.WasPlaying && !candidate.WasEmissionEnabled)
        {
            return;
        }

        emission.enabled = false;
        candidate.ParticleSystem.Clear(withChildren: false);
        candidate.ParticleSystem.Pause(withChildren: false);
        candidate.Suspended = true;
    }

    private static void ResumeParticle(ParticleCandidate candidate)
    {
        if (!candidate.Suspended || candidate.ParticleSystem == null)
        {
            return;
        }

        ParticleSystem.EmissionModule emission = candidate.ParticleSystem.emission;
        emission.enabled = candidate.WasEmissionEnabled;

        if (candidate.WasPlaying && candidate.ParticleSystem.gameObject.activeInHierarchy)
        {
            candidate.ParticleSystem.Play(withChildren: false);
        }

        candidate.Suspended = false;
    }

    private static void SuspendRenderer(RendererCandidate candidate)
    {
        if (candidate.Suspended || candidate.Renderer == null || !candidate.Renderer.enabled)
        {
            return;
        }

        candidate.WasEnabled = candidate.Renderer.enabled;
        candidate.Renderer.enabled = false;
        candidate.Suspended = true;
    }

    private static void ResumeRenderer(RendererCandidate candidate)
    {
        if (!candidate.Suspended || candidate.Renderer == null)
        {
            return;
        }

        candidate.Renderer.enabled = candidate.WasEnabled;
        candidate.Suspended = false;
    }

    private static void SuspendBehaviour(BehaviourCandidate candidate)
    {
        if (candidate.Suspended || candidate.Behaviour == null || !candidate.Behaviour.enabled)
        {
            return;
        }

        candidate.WasEnabled = candidate.Behaviour.enabled;
        candidate.Behaviour.enabled = false;
        candidate.Suspended = true;
    }

    private static void ResumeBehaviour(BehaviourCandidate candidate)
    {
        if (!candidate.Suspended || candidate.Behaviour == null)
        {
            return;
        }

        candidate.Behaviour.enabled = candidate.WasEnabled;
        candidate.Suspended = false;
    }

    private void RestoreAllOnceWhileDisabled()
    {
        if (_restoredWhileDisabled)
        {
            return;
        }

        RestoreAll();
        _restoredWhileDisabled = true;
    }

    private void RestoreAll()
    {
        RestoreParticles();
        RestoreRenderers();
        RestoreBehaviours();
    }

    private void RestoreParticles()
    {
        foreach (ParticleCandidate candidate in _particleCandidates)
        {
            ResumeParticle(candidate);
        }
    }

    private void RestoreRenderers()
    {
        foreach (RendererCandidate candidate in _rendererCandidates)
        {
            ResumeRenderer(candidate);
        }
    }

    private void RestoreBehaviours()
    {
        foreach (BehaviourCandidate candidate in _behaviourCandidates)
        {
            ResumeBehaviour(candidate);
        }
    }

    private void PruneDestroyedCandidates()
    {
        PruneDestroyed(_particleCandidates, _particleCandidateIds, candidate => candidate.ParticleSystem);
        PruneDestroyed(_rendererCandidates, _rendererCandidateIds, candidate => candidate.Renderer);
        PruneDestroyed(_behaviourCandidates, _behaviourCandidateIds, candidate => candidate.Behaviour);
    }

    private static void PruneDestroyed<T>(List<T> candidates, HashSet<int> ids, Func<T, UnityEngine.Object?> getObject)
        where T : CandidateBase
    {
        for (int i = candidates.Count - 1; i >= 0; i--)
        {
            T candidate = candidates[i];
            if (getObject(candidate) != null)
            {
                continue;
            }

            ids.Remove(candidate.Id);
            candidates.RemoveAt(i);
        }
    }

    private static bool IsSafeVisualCandidate(GameObject gameObject)
    {
        if (gameObject == null || HasRiskyTagOrName(gameObject))
        {
            return false;
        }

        if (gameObject.GetComponentInParent<Rigidbody2D>() != null ||
            gameObject.GetComponentInParent<Collider2D>() != null ||
            gameObject.GetComponentInChildren<Rigidbody2D>(includeInactive: true) != null ||
            gameObject.GetComponentInChildren<Collider2D>(includeInactive: true) != null)
        {
            return false;
        }

        foreach (MonoBehaviour component in gameObject.GetComponentsInParent<MonoBehaviour>(includeInactive: true))
        {
            if (IsRiskyComponent(component))
            {
                return false;
            }
        }

        foreach (MonoBehaviour component in gameObject.GetComponentsInChildren<MonoBehaviour>(includeInactive: true))
        {
            if (IsRiskyComponent(component))
            {
                return false;
            }
        }

        return true;
    }

    private static bool IsRiskyComponent(MonoBehaviour component)
    {
        if (component == null || component is OffscreenCullingCoordinator)
        {
            return false;
        }

        string typeName = component.GetType().Name;
        for (int i = 0; i < RiskyNameParts.Length; i++)
        {
            if (typeName.IndexOf(RiskyNameParts[i], StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return true;
            }
        }

        return false;
    }

    private static bool HasRiskyTagOrName(GameObject gameObject)
    {
        if (ContainsRiskyText(gameObject.name))
        {
            return true;
        }

        try
        {
            return ContainsRiskyText(gameObject.tag);
        }
        catch (UnityException)
        {
            return false;
        }
    }

    private static bool ContainsRiskyText(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return false;
        }

        for (int i = 0; i < RiskyNameParts.Length; i++)
        {
            if (text.IndexOf(RiskyNameParts[i], StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return true;
            }
        }

        return false;
    }

    private static Camera? GetMainCamera()
    {
        GameCameras cameras = GameCameras.SilentInstance;
        if (cameras != null && cameras.mainCamera != null)
        {
            return cameras.mainCamera;
        }

        return Camera.main;
    }

    private static bool IsVisibleOrProtected(Bounds bounds, Transform transform, Camera camera)
    {
        if (transform == null)
        {
            return true;
        }

        float nearHeroDistance = ModSettings.OffscreenCullingNearHeroDistance.Value;
        if (IsNearHero(transform.position, nearHeroDistance))
        {
            return true;
        }

        return IsInsideExpandedCameraBounds(bounds, transform, camera);
    }

    private static bool IsNearHero(Vector3 position, float distance)
    {
        if (distance <= 0f)
        {
            return false;
        }

        HeroController hero = HeroController.instance;
        if (hero == null)
        {
            return false;
        }

        return (hero.transform.position - position).sqrMagnitude <= distance * distance;
    }

    private static bool IsInsideExpandedCameraBounds(Bounds bounds, Transform transform, Camera camera)
    {
        float padding = ModSettings.OffscreenCullingViewportPadding.Value;
        if (camera.orthographic)
        {
            Vector3 cameraPosition = camera.transform.position;
            float halfHeight = camera.orthographicSize + padding;
            float halfWidth = halfHeight * camera.aspect + padding;
            Bounds testBounds = HasValidBounds(bounds) ? bounds : CreateFallbackBounds(transform);

            return testBounds.max.x >= cameraPosition.x - halfWidth &&
                   testBounds.min.x <= cameraPosition.x + halfWidth &&
                   testBounds.max.y >= cameraPosition.y - halfHeight &&
                   testBounds.min.y <= cameraPosition.y + halfHeight;
        }

        Vector3 viewportPoint = camera.WorldToViewportPoint(transform.position);
        float viewportPadding = Mathf.Clamp01(padding * 0.02f);
        return viewportPoint.z > 0f &&
               viewportPoint.x >= -viewportPadding &&
               viewportPoint.x <= 1f + viewportPadding &&
               viewportPoint.y >= -viewportPadding &&
               viewportPoint.y <= 1f + viewportPadding;
    }

    private static Bounds GetCandidateBounds(Renderer? renderer, Transform transform, Bounds lastBounds, bool suspended)
    {
        if (!suspended && renderer != null && TryReadBounds(renderer, transform, out Bounds rendererBounds))
        {
            return rendererBounds;
        }

        if (HasValidBounds(lastBounds))
        {
            return new Bounds(transform.position, lastBounds.size);
        }

        return CreateFallbackBounds(transform);
    }

    private static bool TryReadBounds(Renderer renderer, Transform transform, out Bounds bounds)
    {
        bounds = renderer.bounds;
        if (HasValidBounds(bounds))
        {
            return true;
        }

        bounds = CreateFallbackBounds(transform);
        return false;
    }

    private static Bounds CreateFallbackBounds(Transform transform)
    {
        return new Bounds(transform != null ? transform.position : Vector3.zero, Vector3.one);
    }

    private static bool HasValidBounds(Bounds bounds)
    {
        Vector3 size = bounds.size;
        return size.sqrMagnitude > 0.0001f &&
               !float.IsNaN(size.x) &&
               !float.IsNaN(size.y) &&
               !float.IsNaN(size.z);
    }

    private abstract class CandidateBase
    {
        protected CandidateBase(int id)
        {
            Id = id;
        }

        public int Id { get; }
    }

    private sealed class ParticleCandidate : CandidateBase
    {
        public ParticleCandidate(int id, ParticleSystem particleSystem, Renderer? renderer, Bounds lastBounds)
            : base(id)
        {
            ParticleSystem = particleSystem;
            Renderer = renderer;
            LastBounds = lastBounds;
        }

        public ParticleSystem ParticleSystem { get; }

        public Renderer? Renderer { get; }

        public Bounds LastBounds { get; }

        public bool Suspended { get; set; }

        public bool WasPlaying { get; set; }

        public bool WasEmissionEnabled { get; set; }
    }

    private sealed class RendererCandidate : CandidateBase
    {
        public RendererCandidate(int id, Renderer renderer, Bounds lastBounds)
            : base(id)
        {
            Renderer = renderer;
            LastBounds = lastBounds;
        }

        public Renderer Renderer { get; }

        public Bounds LastBounds { get; }

        public bool Suspended { get; set; }

        public bool WasEnabled { get; set; }
    }

    private sealed class BehaviourCandidate : CandidateBase
    {
        public BehaviourCandidate(int id, Behaviour behaviour, Renderer anchorRenderer, Bounds lastBounds)
            : base(id)
        {
            Behaviour = behaviour;
            AnchorRenderer = anchorRenderer;
            LastBounds = lastBounds;
        }

        public Behaviour Behaviour { get; }

        public Renderer AnchorRenderer { get; }

        public Bounds LastBounds { get; }

        public bool Suspended { get; set; }

        public bool WasEnabled { get; set; }
    }
}
