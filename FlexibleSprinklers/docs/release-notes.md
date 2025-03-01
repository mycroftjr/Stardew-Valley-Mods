[← back to readme](README.md)

# Release notes

## 1.3.2
Released 3 August 2022.

* Fixed "Could not find the location the sprinkler is in." errors being written all the time.
* Super smallish performance improvements.
* Added an API to retrieve the mod's read-only config.

## 1.3.1
Released 21 June 2022.

* Fixed compatibility with the Solid Foundations framework.

## 1.3.0
Released 19 June 2022.

* Added an option to allow watering Garden Pots.
* Added an option to allow watering pet bowls.
* Added an option to ignore sprinklers' range.
* Changed custom behaviors to also take into account the unmodified coverage shape of the sprinklers - this improves behavior for Line Sprinklers.
* Changed default behavior from "Vanilla > Cluster" to "Cluster".
* Fixed tools/weapons being used when right clicking on sprinklers to activate them or display the coverage overlay.
* Obsoleted the "Flood fill"-family behaviors - they are still available, but will be removed in a future update.
* Added additional APIs for modders: `GetAllTilesInRangeOfSprinklers`, `GetSprinklerSpreadRange`, `GetSprinklerFocusedRange` and `GetSprinklerMaxRange`.
* Obsoleted `GetFloodFillSprinklerRange` API for modders.

## 1.2.5
Released 16 April 2022.

* Fixed Slime Hutch water troughs not being watered with the Cluster behavior.
* Fixed "optional" translations (usually tooltips) always using English in GMCM integration.

## 1.2.4
Released 25 February 2022.

* Performance improvements.

## 1.2.3
Released 24 February 2022.

* Added a new "Split disconnected clusters" setting (enabled by default).
* Fixed "Show coverage on placement" being triggered by any objects, not just sprinklers.
* "Fixed" Enrichers for Cluster behavior (this actually cannot work easily, but the "fix" is "good enough").
* General code improvements and optimizations.

## 1.2.2
Released 24 February 2022.

* Fixed coverage animation when there are multiple clusters while using the Cluster behavior.

## 1.2.1
Released 24 February 2022.

* Fixed behavior if the sprinklers were too good for a given cluster.

## 1.2.0
Released 24 February 2022.

* Added Cluster (with or without vanilla) behavior, with associated options.
* Added an option to activate sprinklers before sleep.
* Implemented sprinkler coverage rendering, with associated options.
* Behaviors now actually work in "steps", which can be animated via coverage rendering.
* Fixed Slime Hutch water troughs not being watered.
* Added localization (i18n) support.
* Performance improvements (caching).
* Changed some default settings.
* Added additional APIs for modders.

## 1.1.2
Released 14 February 2022.

* Fixed issues with Prismatic Tools sprinklers.
* Stopping any already playing animations when activating sprinklers manually.

## 1.1.1
Released 6 February 2022.

* Fixed GMCM not being actually optional.

## 1.1.0
Released 4 February 2022.

* Implemented proper "Exact" edge case handling, spiraling around the sprinkler.
* Potential crash fixes.
* "Compatibility Mode" patching for Line Sprinklers.

## 1.0.0
Released 3 February 2022.

* Initial release.