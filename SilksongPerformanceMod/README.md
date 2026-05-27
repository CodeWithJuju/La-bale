# Performance

Plugin `BepInEx` focado em mitigar gargalos de runtime que sao viaveis por patch externo sobre o codigo decompilado.

## Changelog 0.2.5

- `OffscreenCullingCoordinator`: reduziu overhead do proprio mod. Candidatos rejeitados agora sao cacheados e nao passam novamente pelo filtro caro a cada scan.
- `OffscreenCullingCoordinator`: removeu varredura generica de todos os `Renderer`; renderers agora entram no culling pesado apenas quando pertencem a behaviours decorativos ja classificados como seguros.
- `OffscreenCulling`: scan padrao ficou mais espacada (`180` frames, com minimo interno de `120`) e o budget padrao caiu para `80` candidatos por frame.
- `WeaverWalkThread`: fast path opcional usando distancia ao quadrado; so calcula `sqrt` quando precisa interpolar alpha.
- `WaterPhysics`: sem mudanca nesta versao; o patch de buffers reutilizados ja existia.

## Changelog 0.2.4

- `OffscreenCullingCoordinator`: novo culling pesado para trabalho visual fora da tela, com margem de seguranca ao redor da camera e protecao por distancia da Hornet.
- Particulas loopadas seguras: pausa, limpa e desliga emissao enquanto estao fora da tela; one-shot particles nao sao pausadas para nao travar reciclagem.
- Renderers visuais seguros: desliga `Renderer.enabled` fora da tela quando o objeto nao tem colisores, rigidbody, IA, dano, UI, audio ou componentes de gameplay conhecidos.
- Behaviours decorativos seguros: suspende `AmbientFloat`, `AmbientSway`, `ColourDistanceSilhouette`, `FloatingObject`, `JitterSelfSimple`, `LoopRotator`, `SpriteFadePulse` e `TK2DSpriteFadePulse` fora da tela.
- Tudo fica em config `OffscreenCulling`, com toggle geral e toggles separados para particulas, renderers e behaviours.

## Changelog 0.2.3

- `WaterfallParticles`: reduz checks distantes de splash/raycast sem desligar o efeito perto do heroi ou da camera.
- `ParticleSystemAutoDisable`: reduz polling de `IsAlive()`; efeitos finalizados podem reciclar alguns frames mais tarde, sem mudar o visual ativo.
- `Process.EnableHighProcessPriority`: opcional e desligado por padrao; permite subir a prioridade do processo para `High` enquanto o plugin esta carregado.

## Changelog 0.2.2

- `PooledEffectManager`: skip idle O(1) via contador (sem reflection por frame).
- GPU agressivo desligado por padrao; restaura settings ao descarregar plugin.
- `PlanarRealtimeReflection`: registry em OnEnable (sem `FindObjectsOfTypeAll` em load).
- `LagHitRegistry`: limpeza ao destruir source + cancel lag hit sem scan reflexivo extra.
- Harmony priorities (GC/combate cedo, post-process tarde).
- Config GC/GPU/Memory reaplica em runtime (`SettingChanged`).
- Throttle por distancia so em gameplay (menu/load sem interferencia).
- `SceneMemoryCoordinator`: protecao contra double-hook.

## O que esta linha estavel ataca

- `GCManager`: mantem o GC automatico ligado e evita `ForceCollect` bloqueante durante gameplay.
- `IdleMemoryCleaner`: substitui coleta bloqueante/compacting por coleta mais leve.
- `ParticleCulling`: reduz a frequencia de `LateUpdate` para raizes distantes.
- `WaterfallParticles`: reduz a frequencia de raycasts de splash para quedas d'agua distantes.
- `ParticleSystemAutoDisable`: reduz a frequencia do polling de efeitos de particula ja ativos.
- `OffscreenCulling`: remove trabalho visual seguro fora de tela sem desligar objetos de gameplay.
- `SceneColorManager`: limita a frequencia de `UpdateScript` quando a atualizacao nao e forcada.
- `AlertRange` e `LineOfSightDetector`: reduz checks de LOS para inimigos distantes.
- `Walker`: reduz a frequencia de `Update` para walkers distantes do heroi e da camera.
- `HeroAudioController`: elimina chamadas redundantes de estado de passos ja ativo/inativo.
- `AmbientFloat`: reduz a frequencia de oscilacao decorativa para objetos distantes e fora da area relevante.
- `AmbientSway`: reduz a frequencia de sway decorativo para objetos distantes e fora da area relevante.
- `ColourDistanceSilhouette`: reduz recalculo de cor por distancia para objetos distantes e fora da area relevante.
- `PooledEffectManager`: pula `Update` quando nao existe nenhum efeito pooled aguardando release atrasado.
- Modo agressivo de GPU: forca video baixo e reduz custo de qualidade secundario, mas preserva o stack principal de bloom/blur/deband e pos-processo.
- `tk2dUpdateManager`: evita entradas duplicadas na fila de commit de texto dentro da mesma janela de flush.
- `AraTrail`: pula rebuild de mesh quando o trail esta fora da tela e longe da camera ativa.
- `NestedFadeGroupMixer`: reduz a frequencia de `LateUpdate` para mixers distantes durante gameplay.
- `DamageEnemies`: pula overlap rescans de cooldown para multihit quando nenhum collider novo entrou no hitbox.
- `HealthManager.CancelAllLagHitsForSource`: usa um indice por origem de lag hit para evitar scan completo em todos os inimigos ativos.
- `UberPostprocess`: faz cache da lista de modulos de post-processo em vez de redescobrir os componentes a cada render.
- `RealtimeReflections`: reduz cubemap e espalha atualizacao das faces ao longo dos frames.
- `LightBlurredBackground`: limita a altura da render texture usada pelo blur de fundo, mantendo o efeito ativo.
- `CameraRenderToMesh`: reduz frequencia de checagem de redimensionamento de render texture.
- `AudioEventManager`, `LowPassDistance` e `AnimatorGroup`: pulam trabalho quando estao sem estado ativo para processar.
- Limpeza de memoria em transicao de cena: forca unload de assets nao usados em intervalos configuraveis, sem remover elementos visiveis da cena ativa.
- Buffer de upload de texturas: reduz o buffer assincrono reservado pelo Unity para diminuir memoria sem alterar qualidade das texturas.
- `WaterPhysics`: remove alocacoes temporarias por tick de fisica reutilizando buffers internos.
- Prioridade alta do processo: opcional via config, restaura a prioridade anterior ao descarregar ou desativar.

## O que ficou de fora nesta versao

- `HeroController` alem de footstep debounce: o metodo e grande demais para uma intervencao segura sem profiler/captures e sem o projeto Unity completo.
- Reescrita pesada de `HealthManager.Update` e do fluxo principal de dano: continua sendo a parte mais arriscada para regressao de combate e ainda nao foi substituida integralmente.
- GPU full-screen effects (`BloomOptimized`, draw calls, shaders): sem assets e sem pipeline completo, o mod preserva o visual e trabalha mais por tuning/caching do que por desativacao bruta.

## Build

O projeto assume por padrao:

- jogo em `C:\Program Files (x86)\Steam\steamapps\common\Hollow Knight Silksong`
- `BepInEx` ja instalado no jogo

Se o seu caminho for diferente:

```powershell
dotnet build .\SilksongPerformanceMod\SilksongPerformanceMod.csproj -c Release -p:GameDir="D:\Seu\Caminho\Hollow Knight Silksong"
```

## Instalacao

Copie `Performance.dll` de `bin\Release\netstandard2.1\` para:

```text
<GameDir>\BepInEx\plugins\
```

## Configuracao

Na primeira execucao o plugin gera um arquivo de config do `BepInEx`. O mod nao usa presets de desempenho; as otimizacoes estaveis ficam ativas por padrao. Se algum comportamento causar regressao em uma area especifica, desative so a chave correspondente no arquivo de config.
