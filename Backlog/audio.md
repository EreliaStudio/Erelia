# Audio

See [global.md](global.md) for the epic summary.

| Priority | Pts | Ticket | Notes |
|----------|-----|--------|-------|
| 110 | 2 | **[AU-01] `AudioManager` singleton MonoBehaviour — SFX and BGM channels** | Persistent singleton (DontDestroyOnLoad). Two `AudioSource` components: one for SFX (non-looping, polyphonic via `PlayOneShot`), one for BGM (looping). Accessible statically via `AudioManager.Instance`. |
| 110 | 1 | **[AU-02] `PlaySFX(AudioClip)` and `PlayBGM(AudioClip)` public methods** | `PlaySFX`: calls `AudioSource.PlayOneShot`. `PlayBGM`: stops current BGM, assigns new clip, plays. Both are null-safe no-ops if `AudioManager.Instance` is null. |
| 110 | 1 | **[AU-03] BGM fade in / fade out coroutine** | `FadeInBGM(clip, duration)` and `FadeOutBGM(duration)`. Lerp the BGM `AudioSource.volume` over the given duration. `FadeInBGM` starts playback at volume 0 and ramps up; `FadeOutBGM` ramps down then stops. |
| 85 | 1 | **[AU-04] Subscribe `AudioManager` to `BattleStarted` / `BattleEnded` events** | `AudioManager` (or a `BattleMusicController`) subscribes to `EventCenter.BattleStarted` → `FadeInBGM(battleClip)`, and `EventCenter.BattleEnded` → `FadeOutBGM` then restore exploration BGM. |
| 85 | 1 | **[AU-05] Assign / author battle BGM clip** | Assign a placeholder or final audio clip to the `BattleMusicController` or directly to a field on `AudioManager`. The clip plays on battle entry. |
| 65 | 1 | **[AU-06] `PlaySoundStep` triggers `AudioManager.PlaySFX`** | Complete the stub from VA-19: call `AudioManager.Instance?.PlaySFX(clip)` inside `PlaySoundStep.Execute`. Requires AU-01. |
| 65 | 1 | **[AU-07] Assign SFX clips to authored abilities** | For each `Ability` SO from CO-04 to CO-09, assign an `AudioClip` to the `PlaySoundStep` in its caster recipe (or to a dedicated SFX field on `Ability` if no recipe is used yet). |
| 50 | 1 | **[AU-08] Button click SFX via `AudioManager`** | Add a shared `UiSfxController` MonoBehaviour (or an `EventTrigger` component) that calls `AudioManager.PlaySFX(clickClip)` on every UI button click. Applied globally via a `GraphicRaycaster` event or base button class. |
| 50 | 1 | **[AU-09] Menu navigation SFX** | Separate `AudioClip` for hover/selection change events in menus. `UiSfxController` subscribes to pointer-enter events on interactive UI elements and plays the navigation clip. |
| 45 | 2 | **[AU-10] `FootstepEmitter` MonoBehaviour — SFX on move step event** | Attached to the player actor. Subscribes to the exploration movement event (or an animation frame event). Calls `AudioManager.PlaySFX(footstepClip)` on each step. Clip can vary by terrain voxel type if desired. |
