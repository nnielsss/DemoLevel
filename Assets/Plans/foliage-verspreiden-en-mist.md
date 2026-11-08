# Project Overview
- Game Title: Niet opgegeven; bestaand DemoLevel-project.
- High-Level Concept: De bestaande NPC-terreinscene aankleden met het zelf geïmporteerde foliage-model, dicht verspreide begroeiing en sfeervolle mist. Het bestaande landschap en de bestaande NPC-logica behouden.
- Players: Bestaande NPC-demo; spelersmodus niet gewijzigd.
- Inspiration / Reference Games: Niet opgegeven; niet nodig voor deze omgevingswijziging.
- Tone / Art Direction: Door gebruiker gekozen: dicht begroeid en mistig.
- Target Platform: StandaloneWindows64.
- Screen Orientation / Resolution: Bestaande Game View behouden; geen nieuwe resolutie instellen.
- Render Pipeline: URP, PC_RPAsset; Unity 6000.6.5f1.

# Game Mechanics
## Core Gameplay Loop
Bestaande NPC’s wandelen over het NavMesh; hun gedrag blijft ongewijzigd. Begroeiing en afstandsmist geven de omgeving meer sfeer. Open plekken rond het kampvuur en de NPC-startposities behouden zodat belangrijke objecten herkenbaar blijven. In MDA-termen veranderen vooral de esthetiek en zichtbaarheid, niet de onderliggende bewegingsmechaniek.

## Controls and Input Methods
Projectcontext vermeldt New Input System. Geen bediening toevoegen of wijzigen; bestaande camera met CameraFollowNPC en NPCWander behouden.

# UI
Geen UI toevoegen of wijzigen. Visuele compositie in de Game View:

```text
[ mistige achtergrond / vervagende verre begroeiing ]
[ dichte onregelmatige plantengroepen op het terrein ]
[ leesbare NPC’s / open ruimte rond het kampvuur     ]
```
Mist in de daadwerkelijke volgcamera beoordelen, niet alleen vanuit de hoge editor-camerapositie.

# Key Asset & Context
## Gecontroleerde toestand
- Actieve scene: `Assets/Scenes/NpcScene.unity`.
- Bronmodel: `Assets/Eigen Modellen/Foliage.fbx`; nog geen afhankelijke scenes/prefabs gevonden.
- Model bevat twee renderers: Foliage en Stem. Beide gebruiken URP/Lit-materialen (Leaf_GreenBeans_PBR en Stem_Green).
- Modelbounds lopen onder de modeloorsprong; voet/pivot corrigeren in een wrapperprefab om zweven of diepe begraving te voorkomen. De bladeren zijn vrijwel vlak: zichtbaarheid van de achterkant controleren.
- DemoTerrain heeft Terrain en TerrainCollider, maar beide hebben momenteel geen laadbare TerrainData.
- Scene verwijst naar GUID `23ff03fb27bc96f4b9fd1ada2e4b4a5f`, dat wel naar `Assets/New Terrain 1.asset` resolveert. LoadAllAssetsAtPath levert daarvoor nul objecten; ook `Assets/New Terrain.asset` levert nul objecten. Oorzaak nog niet vastgesteld; niet als bewezen corruptie behandelen.
- Andere pakket-terreinen zijn laadbaar, maar zijn geen bewezen kopieën van het oorspronkelijke landschap. Deze niet stilzwijgend gebruiken als vervanging.
- Mist is al ingeschakeld: Linear, grijs, start 20 en einde 100. Deze mist afstellen in plaats van een tweede effect stapelen.
- Bestaande assets: `Assets/Prefabs/Npc_01.prefab`, `Assets/Scripts/NPCWander.cs`, `Assets/Scripts/CameraFollowNPC.cs`, `Assets/Scenes/NpcScene/NavMesh-Navigation.asset`.

## Keuzes en grenzen
Gebruiker koos bestaand terrain herstellen en dicht/mistig. Geen nieuw terrein genereren en geen pakketlandschap substitueren zonder extra toestemming.

Voor verspreiding zijn twee technische routes mogelijk:
1. Wrapperprefab-instances in de scene: ondersteunt het tweedelige model zonder samenvoegen, eenvoudig te inspecteren en te verwijderen; meer sceneobjecten/draw calls. Aanbevolen voor deze afgebakende aankleding.
2. Terrain-details met instancing: schaalbaar bij zeer grote aantallen, maar vraagt extra controle van model/renderer-compatibiliteit en mogelijk een samengevoegd mesh. Niet standaard gekozen om het bronmodel ongemoeid te laten.

## Te wijzigen / maken
- Wijzigen: `Assets/Scenes/NpcScene.unity`: TerrainData-koppelingen indien herstel lukt, foliage-root en prefabinstances, scene-RenderSettings voor mist.
- Mogelijk herstellen via gerichte herimport: `Assets/New Terrain 1.asset`; bronbestand en meta/GUID behouden. Vooraf backup maken en afhankelijkheden/afhankelijke scenes controleren.
- Maken: `Assets/Prefabs/Foliage_Terrain.prefab`, met neutrale root en genest origineel model inclusief voetcorrectie.
- Alleen indien nodig: eigen materiaalkopieën onder `Assets/Materials/Foliage/` voor tweezijdige bladeren; ingebedde FBX-materialen of gedeelde pakketmaterialen niet globaal wijzigen.
- Geen wijzigingen aan gameplayscripts, URP-packagecode, renderpipelineassets of bestaande NavMesh-assets gepland.

# Implementation Steps
1. **Herstel eerst de terrainbasis.**
   - Dependencies en dependents van NpcScene, New Terrain 1.asset en het foliage-model opnieuw controleren voordat wijzigingen plaatsvinden.
   - Bestandsgrootte, importstatus, consolefouten en eventueel bestaande herstel-/backupbronnen onderzoeken; de scene bevat de oorspronkelijke GUID nog.
   - Backup van de betreffende scene, TerrainData en metadata maken; daarna uitsluitend het bedoelde terrainbestand gericht herimporteren.
   - Wanneer dezelfde TerrainData weer laadbaar is: aan zowel DemoTerrain.Terrain als TerrainCollider toewijzen, zonder hoogtekaart of terreinlagen te vervangen.
   - Als herstel niet lukt: stoppen met plaatsing, het specifieke probleem melden en vragen om een originele backup of expliciete toestemming voor een vervangend landschap. De andere pakket-terreinen niet automatisch koppelen.
   - **Dependencies:** Geen; harde voorwaarde voor stappen 2–4.

2. **Foliage-wrapper maken en één exemplaar testen.**
   - `Assets/Prefabs/Foliage_Terrain.prefab` maken met genest `Assets/Eigen Modellen/Foliage.fbx`.
   - Root op terreinvoet plaatsen en modelchild verticaal corrigeren op basis van daadwerkelijke meshbounds.
   - Bestaande URP-materialen behouden. Bij onzichtbare bladachterkanten alleen het bladmaterial kopiëren en tweezijdig instellen op de kopie.
   - Geen colliders/NavMeshObstacles toevoegen aan deze decoratieve planten.
   - Eén exemplaar op het herstelde terrain testen voor schaal, voetcontact, bladzichtbaarheid en schaduwen.
   - **Dependencies:** Stap 1 succesvol; materiaalafhankelijkheden controleren vóór eventuele materiaalkopie/-wijziging.

3. **Dicht en onregelmatig verspreiden.**
   - In NpcScene een duidelijke root `Foliage_Scatter` maken; prefabverbindingen behouden.
   - Reproduceerbare posities met vaste seed kiezen binnen de daadwerkelijke terreinbounds; hoogte samplen via Terrain.SampleHeight plus terreinoffset.
   - Dichte clusters afwisselen met kleinere open plekken, willekeurige Y-rotatie en schaalvariatie ongeveer 0,8–1,2 gebruiken.
   - Zeer steile hellingen, gaten, terreinranden, kampvuurzone en directe NPC-startplekken vermijden. Minimaal circa 3 meter rondom het kampvuur vrijhouden, bijstellen op basis van werkelijke objectbounds.
   - Aantal pas bepalen zodra terreinoppervlak bekend is: beginnen met een beperkte proefgroep, daarna tot een overtuigend dichte begroeiing uitbreiden met een initiële bovengrens van circa 1.000 instances. Geen duizenden exemplaren blind toevoegen.
   - Geen navigatieblokkering of NavMesh-herbak nodig voor colliderloze decoratie.
   - Bij herhaling alleen de eigen gegenereerde groep vervangen, geen andere begroeiing verwijderen.
   - **Dependencies:** Stappen 1 en 2.

4. **Bestaande mist versterken en afstellen.**
   - Scene RenderSettings gebruiken voor gewone URP-afstandsmist; geen volumetrische mist of nieuw Volume-/pakket nodig.
   - ExponentialSquared als uitgangspunt nemen met een gedempte grijsblauwe tint en initiële density circa 0,025; aanpassen aan landschapgrootte en volgcamera-afstand.
   - Een leesbare voorgrond behouden terwijl verre begroeiing duidelijk vervaagt. Bestaande belichting, kampvuursmoke en skybox behouden; horizonovergang visueel controleren.
   - Geen projectbrede shaderstripping-instellingen wijzigen; eerst het effect in Game View verifiëren en eventuele buildafwijkingen rapporteren.
   - **Dependencies:** Stap 3 en werkende terrainbasis.

5. **Valideren en opslaan.**
   - Prefab en actieve scene opslaan nadat visuele en technische controles slagen.
   - Aantal geplaatste planten, definitieve mistinstellingen en eventueel terrainherstel rapporteren.
   - **Dependencies:** Stappen 1–4.

# Verification & Testing
- Beide terraincomponenten verwijzen naar dezelfde geldige oorspronkelijke TerrainData; terreinlagen en hoogten blijven behouden.
- Bij onherstelbare data stopt uitvoering zonder een vervangend landschap of foliage op een ongeldige ondergrond op te slaan.
- Planten staan binnen het terrein, niet boven gaten of met zwevende voeten; correcte plaatsing op heuvels en bij terreinoffset controleren.
- Zowel blad als steel zichtbaar; geen magenta materialen en geen onzichtbare achterkanten tijdens camerabeweging.
- Rotatie, schaal en clusterpatronen leveren dicht begroeide maar niet uniform geroosterde verspreiding op.
- Kampvuur blijft vrij en herkenbaar; NPC’s bewegen nog met hun bestaande scripts en NavMesh.
- Game View in Play Mode controleren met CameraFollowNPC; mist is zichtbaar, de voorgrond blijft leesbaar en de horizon sluit acceptabel aan.
- Console op nieuwe fouten controleren; bestaande pakket-terrainwaarschuwingen onderscheiden van nieuw geïntroduceerde problemen.
- Renderkosten van een proefgroep vergelijken met de uiteindelijke groep; bij duidelijke regressie aantal/draw distance verminderen, niet ongevraagd een nieuw rendersysteem bouwen.
- Scene na opslaan heropenen en controleren dat prefabinstances en mistinstellingen persistent zijn.
- Indien een Windows-developmentbuild beschikbaar is, mist en materialen ook daarin controleren; een niet-uitgevoerde buildtest expliciet als niet getest rapporteren.
