using System.Collections.Generic;
using ICities;
using NaturalDisastersRenewal.Common.enums;

namespace NaturalDisastersRenewal.Common;

public static class LocalizationService
{
    private static readonly Dictionary<ModLanguage, Dictionary<string, string>> Translations =
        new()
        {
            {
                ModLanguage.English, new Dictionary<string, string>
                {
                        ["language.english"] = "English",
                        ["language.spanish"] = "Spanish",
                        ["key.ctrl"] = "Ctrl",
                        ["key.alt"] = "Alt",
                        ["key.shift"] = "Shift",
                        ["key.command"] = "Cmd",
                        ["key.up"] = "Up",
                        ["key.down"] = "Down",
                        ["key.left"] = "Left",
                        ["key.right"] = "Right",
                        ["key.pageUp"] = "Page Up",
                        ["key.pageDown"] = "Page Down",
                        ["status.active"] = "Active",
                    ["status.inactive"] = "Inactive",
                    ["status.disabled"] = "Disabled",
                    ["time.and"] = "and",
                    ["time.lessThanOneDay"] = "Less than one day",
                    ["time.day.singular"] = "day",
                    ["time.day.plural"] = "days",
                    ["time.month.singular"] = "month",
                    ["time.month.plural"] = "months",
                    ["time.year.singular"] = "year",
                    ["time.year.plural"] = "years",
                    ["month.january"] = "January",
                    ["month.february"] = "February",
                    ["month.march"] = "March",
                    ["month.april"] = "April",
                    ["month.may"] = "May",
                    ["month.june"] = "June",
                    ["month.july"] = "July",
                    ["month.august"] = "August",
                    ["month.september"] = "September",
                    ["month.october"] = "October",
                    ["month.november"] = "November",
                    ["month.december"] = "December",
                    ["evacuation.manual"] = "Manual evacuation",
                    ["evacuation.auto"] = "Auto evacuation",
                    ["evacuation.focused"] = "Focused auto evacuation/release",
                    ["cracks.none"] = "No cracks",
                    ["cracks.always"] = "Always cracks",
                    ["cracks.byIntensity"] = "Cracks based on strength",
                    ["panel.title"] = "Disasters info",
                    ["panel.tab.statistics"] = "Statistics",
                    ["panel.tab.settings"] = "Settings",
                    ["panel.header.disaster"] = "Disaster",
                    ["panel.header.howOften"] = "How often",
                    ["panel.header.maxStrength"] = "Max strength",
                    ["panel.populationThreshold"] = "Min. population for strongest disasters: {0}.",
                    ["panel.populationThreshold.tooltip"] = "Minimum population needed for the strongest disasters.",
                    ["panel.stopAll"] = "Stop all disasters",
                    ["panel.resetAll"] = "Reset all disasters to their default progress",
                    ["panel.timeFlowNote"] = "Timing adapts to your save's time flow, including Real Time.",
                    ["panel.realTimeStatus"] = "Real Time status: {0}",
                    ["panel.timeOffsetTicks"] = "Time offset ticks: {0}",
                    ["panel.dayTimeFrames"] = "Day-time frames: {0}",
                    ["panel.dayTimeOffsetFrames"] = "Day-time offset frames: {0}",
                    ["settings.group.general"] = "General",
                    ["settings.group.positions"] = "Panel positions",
                    ["settings.group.enableDisasters"] = "Enable disasters",
                    ["settings.group.disaster"] = "{0}",
                    ["settings.group.save"] = "Save options",
                    ["settings.language"] = "Language",
                    ["settings.language.tooltip"] = "Choose the language for panel text and tooltips.",
                    ["settings.evacuationMode"] = "Evacuation mode:",
                    ["settings.disableFollow"] = "Do not follow disasters automatically",
                    ["settings.pauseOnStart"] = "Pause when a disaster starts",
                    ["settings.focusedRadius"] = "Focused evacuation radius",
                    ["settings.maxPopulation"] = "Population needed for top-strength disasters",
                    ["settings.scaleIntensity"] = "Scale max strength with population",
                    ["settings.recordEvents"] = "Record disasters to CSV",
                    ["settings.showPanelButton"] = "Show disaster panel button",
                        ["settings.resetButtonPosition"] = "Reset button position",
                        ["settings.resetPanelPosition"] = "Reset panel position",
                        ["settings.group.hotkey"] = "Hotkey",
                        ["settings.togglePanelHotkey"] = "Toggle panel hotkey",
                        ["settings.hotkeyInfo"] = "Use up to 3 keys: up to 2 modifiers plus 1 regular key.",
                        ["settings.hotkey.capture"] = "Press a key combination...",
                        ["settings.hotkey.none"] = "Not set",
                        ["settings.hotkey.tooltip"] = "Click and then press up to 3 keys. Escape cancels. Backspace or Delete clears the hotkey.",
                        ["settings.hotkey.keypad"] = "Num {0}",
                        ["settings.howOften"] = "How often",
                    ["settings.buildUpTime"] = "Build-up time",
                    ["settings.seasonPeak"] = "Season peak",
                    ["settings.rainBoost"] = "Rain boost",
                    ["settings.groundwaterCapacity"] = "Groundwater capacity",
                    ["settings.noTornadoDuringFog"] = "No tornadoes during fog",
                    ["settings.enableTornadoDestruction"] = "Enable tornado destruction",
                    ["settings.minTornadoDestruction"] = "Min. strength for tornado destruction",
                    ["settings.enableAftershocks"] = "Enable aftershocks",
                    ["settings.groundCracks"] = "Ground cracks:",
                    ["settings.minCrackStrength"] = "Min. crack strength",
                    ["settings.enableLongMeteor"] = "Enable long-period meteors",
                    ["settings.enableMediumMeteor"] = "Enable medium-period meteors",
                    ["settings.enableShortMeteor"] = "Enable short-period meteors",
                    ["settings.saveDefault"] = "Save current settings",
                    ["settings.resetSaved"] = "Reload saved settings",
                    ["settings.resetModDefault"] = "Reset to mod defaults",
                    ["settings.tooltip.timeFlow"] = "Timing adapts to your save's time flow, including Real Time.",
                    ["settings.tooltip.focusedRadius"] = "Radius, in meters, used for focused evacuations.",
                    ["settings.tooltip.maxPopulation"] = "Population needed before the strongest disaster intensities can appear.",
                    ["settings.tooltip.scaleIntensity"] = "Maximum disaster strength starts low and rises as your city grows.",
                    ["settings.tooltip.recordEvents"] = "Writes disaster name, date, and strength to Disasters.csv.",
                    ["settings.tooltip.showPanelButton"] = "Shows or hides the floating button for the disasters panel.",
                    ["settings.tooltip.howOften"] = "Higher values make this disaster more common.",
                    ["settings.tooltip.buildUpTime"] = "How long it takes for this disaster to build up.",
                    ["settings.tooltip.seasonPeak"] = "The time of year when this disaster is most common.",
                    ["settings.tooltip.rainBoost"] = "How much rain increases this disaster.",
                    ["settings.tooltip.groundwaterCapacity"] = "How much rain the ground can hold before sinkholes become more likely.",
                    ["settings.tooltip.noTornadoDuringFog"] = "Prevents tornadoes from appearing during foggy weather.",
                    ["settings.tooltip.enableTornadoDestruction"] = "Allows tornadoes to destroy buildings once they are strong enough.",
                    ["settings.tooltip.minTornadoDestruction"] = "Minimum tornado strength needed before destruction starts.",
                    ["settings.tooltip.enableAftershocks"] = "Large earthquakes can trigger follow-up quakes in the same area.",
                    ["settings.tooltip.groundCracks"] = "Choose whether earthquakes create cracks never, always, or only at higher strength.",
                    ["settings.tooltip.minCrackStrength"] = "Minimum strength needed for ground cracks to appear.",
                    ["settings.tooltip.enableLongMeteor"] = "Allows rare meteors with long approach times.",
                    ["settings.tooltip.enableMediumMeteor"] = "Allows meteors with medium approach times.",
                    ["settings.tooltip.enableShortMeteor"] = "Allows meteors with short approach times.",
                    ["tooltip.notUnlocked"] = "Not unlocked yet",
                    ["tooltip.notUnlockedOutsideArea"] = "Not unlocked yet (can still happen outside your city).",
                    ["tooltip.noDisasterForAnother"] = "No {0} for another {1}",
                    ["tooltip.recentlyOccurred"] = "Lower because {0} happened recently.",
                    ["tooltip.probability"] = "How often: {0}",
                    ["tooltip.intensity"] = "Strength: {0}",
                    ["tooltip.lowPopulation"] = "Lower because your city is still small.",
                    ["tooltip.forestFire.noDuringRain"] = "Forest fires do not happen during rain.",
                    ["tooltip.forestFire.maxNoRain"] = "Highest because there has been no rain for more than {0} days.",
                    ["tooltip.forestFire.increasingNoRain"] = "Rising because there has been no rain for {0}.",
                    ["tooltip.sinkhole.groundwater"] = "Groundwater level: {0}%",
                    ["tooltip.thunderstorm.rainBoost"] = "Higher because of rain.",
                    ["tooltip.thunderstorm.rainBoostCharge"] = "Higher because of rain. Atmospheric charge: {0}%.",
                    ["tooltip.thunderstorm.outOfSeason"] = "Lower because it is outside the storm season.",
                    ["tooltip.thunderstorm.buildingCharge"] = "Storm conditions are building. Atmospheric charge: {0}%.",
                    ["tooltip.thunderstorm.waitingForRain"] = "Season is favorable ({0}%), but it still needs more rain to intensify.",
                    ["tooltip.tornado.noDuringFog"] = "Tornadoes do not happen during fog.",
                    ["tooltip.earthquake.aftershocks"] = "{0} more aftershocks expected.",
                    ["tooltip.meteor.alreadyFallen"] = "{0} already passed.",
                    ["tooltip.meteor.approaching"] = "{0} is approaching.",
                    ["tooltip.meteor.closeIn"] = "{0} will be close in {1}.",
                    ["disaster.earthquake"] = "Earthquake",
                    ["disaster.forestFire"] = "Forest Fire",
                    ["disaster.meteorStrike"] = "Meteor Strike",
                    ["disaster.sinkhole"] = "Sinkhole",
                    ["disaster.thunderstorm"] = "Thunderstorm",
                    ["disaster.tornado"] = "Tornado",
                    ["disaster.tsunami"] = "Tsunami",
                    ["panel.toggleButton.tooltip"] = "Disasters info (drag with right-click)",
                    ["panel.drag.tooltip"] = "Drag with right-click to move this panel."
                }
            },
            {
                ModLanguage.Spanish, new Dictionary<string, string>
                {
                        ["language.english"] = "Ingles",
                        ["language.spanish"] = "Espanol",
                        ["key.ctrl"] = "Ctrl",
                        ["key.alt"] = "Alt",
                        ["key.shift"] = "Shift",
                        ["key.command"] = "Cmd",
                        ["key.up"] = "Arriba",
                        ["key.down"] = "Abajo",
                        ["key.left"] = "Izquierda",
                        ["key.right"] = "Derecha",
                        ["key.pageUp"] = "Re Pag",
                        ["key.pageDown"] = "Av Pag",
                        ["status.active"] = "Activo",
                    ["status.inactive"] = "Inactivo",
                    ["status.disabled"] = "Desactivado",
                    ["time.and"] = "y",
                    ["time.lessThanOneDay"] = "Menos de un dia",
                    ["time.day.singular"] = "dia",
                    ["time.day.plural"] = "dias",
                    ["time.month.singular"] = "mes",
                    ["time.month.plural"] = "meses",
                    ["time.year.singular"] = "ano",
                    ["time.year.plural"] = "anos",
                    ["month.january"] = "Enero",
                    ["month.february"] = "Febrero",
                    ["month.march"] = "Marzo",
                    ["month.april"] = "Abril",
                    ["month.may"] = "Mayo",
                    ["month.june"] = "Junio",
                    ["month.july"] = "Julio",
                    ["month.august"] = "Agosto",
                    ["month.september"] = "Septiembre",
                    ["month.october"] = "Octubre",
                    ["month.november"] = "Noviembre",
                    ["month.december"] = "Diciembre",
                    ["evacuation.manual"] = "Evacuacion manual",
                    ["evacuation.auto"] = "Evacuacion automatica",
                    ["evacuation.focused"] = "Evacuacion automatica enfocada/liberacion",
                    ["cracks.none"] = "Sin grietas",
                    ["cracks.always"] = "Siempre con grietas",
                    ["cracks.byIntensity"] = "Grietas segun la intensidad",
                    ["panel.title"] = "Informacion de desastres",
                    ["panel.tab.statistics"] = "Estadisticas",
                    ["panel.tab.settings"] = "Ajustes",
                    ["panel.header.disaster"] = "Desastre",
                    ["panel.header.howOften"] = "Frecuencia",
                    ["panel.header.maxStrength"] = "Fuerza max.",
                    ["panel.populationThreshold"] = "Poblacion minima para los desastres mas fuertes: {0}.",
                    ["panel.populationThreshold.tooltip"] = "Poblacion minima necesaria para los desastres mas fuertes.",
                    ["panel.stopAll"] = "Detener todos los desastres",
                    ["panel.resetAll"] = "Reiniciar el progreso de todos los desastres",
                    ["panel.timeFlowNote"] = "Los tiempos se adaptan al ritmo del tiempo de tu partida, incluso con Real Time.",
                    ["panel.realTimeStatus"] = "Estado de Real Time: {0}",
                    ["panel.timeOffsetTicks"] = "Ticks de desfase: {0}",
                    ["panel.dayTimeFrames"] = "Frames del ciclo dia/noche: {0}",
                    ["panel.dayTimeOffsetFrames"] = "Frames de desfase del ciclo dia/noche: {0}",
                    ["settings.group.general"] = "General",
                    ["settings.group.positions"] = "Posiciones del panel",
                    ["settings.group.enableDisasters"] = "Activar desastres",
                    ["settings.group.disaster"] = "{0}",
                    ["settings.group.save"] = "Guardado",
                    ["settings.language"] = "Idioma",
                    ["settings.language.tooltip"] = "Elige el idioma de los textos y tooltips.",
                    ["settings.evacuationMode"] = "Modo de evacuacion:",
                    ["settings.disableFollow"] = "No seguir desastres automaticamente",
                    ["settings.pauseOnStart"] = "Pausar cuando empieza un desastre",
                    ["settings.focusedRadius"] = "Radio de evacuacion enfocada",
                    ["settings.maxPopulation"] = "Poblacion necesaria para los desastres mas fuertes",
                    ["settings.scaleIntensity"] = "Escalar fuerza maxima con la poblacion",
                    ["settings.recordEvents"] = "Registrar desastres en CSV",
                    ["settings.showPanelButton"] = "Mostrar boton del panel de desastres",
                        ["settings.resetButtonPosition"] = "Restablecer posicion del boton",
                        ["settings.resetPanelPosition"] = "Restablecer posicion del panel",
                        ["settings.group.hotkey"] = "Atajo",
                        ["settings.togglePanelHotkey"] = "Atajo para mostrar u ocultar el panel",
                        ["settings.hotkeyInfo"] = "Usa hasta 3 teclas: hasta 2 modificadoras y 1 tecla normal.",
                        ["settings.hotkey.capture"] = "Presiona una combinacion...",
                        ["settings.hotkey.none"] = "Sin asignar",
                        ["settings.hotkey.tooltip"] = "Haz clic y luego presiona hasta 3 teclas. Escape cancela. Backspace o Delete borran el atajo.",
                        ["settings.hotkey.keypad"] = "Num {0}",
                        ["settings.howOften"] = "Frecuencia",
                    ["settings.buildUpTime"] = "Tiempo de preparacion",
                    ["settings.seasonPeak"] = "Temporada alta",
                    ["settings.rainBoost"] = "Aumento por lluvia",
                    ["settings.groundwaterCapacity"] = "Capacidad del agua subterranea",
                    ["settings.noTornadoDuringFog"] = "Sin tornados durante la niebla",
                    ["settings.enableTornadoDestruction"] = "Activar destruccion por tornado",
                    ["settings.minTornadoDestruction"] = "Fuerza minima para destruccion por tornado",
                    ["settings.enableAftershocks"] = "Activar replicas",
                    ["settings.groundCracks"] = "Grietas en el suelo:",
                    ["settings.minCrackStrength"] = "Fuerza minima de grietas",
                    ["settings.enableLongMeteor"] = "Activar meteoritos de periodo largo",
                    ["settings.enableMediumMeteor"] = "Activar meteoritos de periodo medio",
                    ["settings.enableShortMeteor"] = "Activar meteoritos de periodo corto",
                    ["settings.saveDefault"] = "Guardar configuracion actual",
                    ["settings.resetSaved"] = "Recargar configuracion guardada",
                    ["settings.resetModDefault"] = "Restablecer valores del mod",
                    ["settings.tooltip.timeFlow"] = "Los tiempos se adaptan al ritmo del tiempo de tu partida, incluso con Real Time.",
                    ["settings.tooltip.focusedRadius"] = "Radio, en metros, usado para evacuaciones enfocadas.",
                    ["settings.tooltip.maxPopulation"] = "Poblacion necesaria para que puedan aparecer las intensidades mas fuertes.",
                    ["settings.tooltip.scaleIntensity"] = "La fuerza maxima empieza baja y sube a medida que tu ciudad crece.",
                    ["settings.tooltip.recordEvents"] = "Guarda nombre, fecha y fuerza del desastre en Disasters.csv.",
                    ["settings.tooltip.showPanelButton"] = "Muestra u oculta el boton flotante del panel de desastres.",
                    ["settings.tooltip.howOften"] = "Los valores altos hacen que este desastre sea mas comun.",
                    ["settings.tooltip.buildUpTime"] = "Cuanto tarda este desastre en prepararse.",
                    ["settings.tooltip.seasonPeak"] = "La epoca del ano en que este desastre es mas comun.",
                    ["settings.tooltip.rainBoost"] = "Cuanto aumenta este desastre cuando llueve.",
                    ["settings.tooltip.groundwaterCapacity"] = "Cuanta lluvia puede retener el suelo antes de que los socavones sean mas probables.",
                    ["settings.tooltip.noTornadoDuringFog"] = "Evita que aparezcan tornados durante el clima con niebla.",
                    ["settings.tooltip.enableTornadoDestruction"] = "Permite que los tornados destruyan edificios cuando tienen suficiente fuerza.",
                    ["settings.tooltip.minTornadoDestruction"] = "Fuerza minima del tornado para empezar a destruir.",
                    ["settings.tooltip.enableAftershocks"] = "Los terremotos grandes pueden provocar replicas en la misma zona.",
                    ["settings.tooltip.groundCracks"] = "Elige si los terremotos crean grietas nunca, siempre o solo con mayor fuerza.",
                    ["settings.tooltip.minCrackStrength"] = "Fuerza minima necesaria para que aparezcan grietas en el suelo.",
                    ["settings.tooltip.enableLongMeteor"] = "Permite meteoritos raros con tiempo de llegada largo.",
                    ["settings.tooltip.enableMediumMeteor"] = "Permite meteoritos con tiempo de llegada medio.",
                    ["settings.tooltip.enableShortMeteor"] = "Permite meteoritos con tiempo de llegada corto.",
                    ["tooltip.notUnlocked"] = "Aun no desbloqueado",
                    ["tooltip.notUnlockedOutsideArea"] = "Aun no desbloqueado (todavia puede ocurrir fuera de tu ciudad).",
                    ["tooltip.noDisasterForAnother"] = "No habra {0} por otros {1}",
                    ["tooltip.recentlyOccurred"] = "Mas bajo porque {0} ocurrio hace poco.",
                    ["tooltip.probability"] = "Frecuencia: {0}",
                    ["tooltip.intensity"] = "Fuerza: {0}",
                    ["tooltip.lowPopulation"] = "Mas bajo porque tu ciudad aun es pequena.",
                    ["tooltip.forestFire.noDuringRain"] = "Los incendios forestales no aparecen durante la lluvia.",
                    ["tooltip.forestFire.maxNoRain"] = "Esta al maximo porque no ha llovido por mas de {0} dias.",
                    ["tooltip.forestFire.increasingNoRain"] = "Va subiendo porque no ha llovido por {0}.",
                    ["tooltip.sinkhole.groundwater"] = "Nivel de agua subterranea: {0}%",
                    ["tooltip.thunderstorm.rainBoost"] = "Es mas alto por la lluvia.",
                    ["tooltip.thunderstorm.rainBoostCharge"] = "Es mas alto por la lluvia. Carga atmosferica: {0}%.",
                    ["tooltip.thunderstorm.outOfSeason"] = "Es mas bajo porque esta fuera de la temporada de tormentas.",
                    ["tooltip.thunderstorm.buildingCharge"] = "Las condiciones para tormenta se estan acumulando. Carga atmosferica: {0}%.",
                    ["tooltip.thunderstorm.waitingForRain"] = "La temporada es favorable ({0}%), pero aun necesita mas lluvia para intensificarse.",
                    ["tooltip.tornado.noDuringFog"] = "Los tornados no aparecen durante la niebla.",
                    ["tooltip.earthquake.aftershocks"] = "Se esperan {0} replicas mas.",
                    ["tooltip.meteor.alreadyFallen"] = "{0} ya paso.",
                    ["tooltip.meteor.approaching"] = "{0} se esta acercando.",
                    ["tooltip.meteor.closeIn"] = "{0} estara cerca en {1}.",
                    ["disaster.earthquake"] = "Terremoto",
                    ["disaster.forestFire"] = "Incendio forestal",
                    ["disaster.meteorStrike"] = "Impacto de meteorito",
                    ["disaster.sinkhole"] = "Socavon",
                    ["disaster.thunderstorm"] = "Tormenta electrica",
                    ["disaster.tornado"] = "Tornado",
                    ["disaster.tsunami"] = "Tsunami",
                    ["panel.toggleButton.tooltip"] = "Informacion de desastres (arrastra con clic derecho)",
                    ["panel.drag.tooltip"] = "Arrastra con clic derecho para mover este panel."
                }
            }
        };

    public static string Get(string key)
    {
        var language = GetCurrentLanguage();
        if (Translations.ContainsKey(language) && Translations[language].ContainsKey(key))
        {
            return Translations[language][key];
        }

        return Translations[ModLanguage.English].ContainsKey(key)
            ? Translations[ModLanguage.English][key]
            : key;
    }

    public static string Format(string key, params object[] args)
    {
        return string.Format(Get(key), args);
    }

    public static string[] GetLanguageDisplayNames()
    {
        return
        [
            GetLanguageDisplayName(ModLanguage.English),
            GetLanguageDisplayName(ModLanguage.Spanish)
        ];
    }

    private static string GetLanguageDisplayName(ModLanguage language)
    {
        return language == ModLanguage.Spanish ? Get("language.spanish") : Get("language.english");
    }

    public static string[] GetMonths()
    {
        return
        [
            Get("month.january"),
            Get("month.february"),
            Get("month.march"),
            Get("month.april"),
            Get("month.may"),
            Get("month.june"),
            Get("month.july"),
            Get("month.august"),
            Get("month.september"),
            Get("month.october"),
            Get("month.november"),
            Get("month.december")
        ];
    }

    public static string GetDisasterName(DisasterType disasterType)
    {
        switch (disasterType)
        {
            case DisasterType.Earthquake:
                return Get("disaster.earthquake");
            case DisasterType.ForestFire:
                return Get("disaster.forestFire");
            case DisasterType.MeteorStrike:
                return Get("disaster.meteorStrike");
            case DisasterType.Sinkhole:
                return Get("disaster.sinkhole");
            case DisasterType.ThunderStorm:
                return Get("disaster.thunderstorm");
            case DisasterType.Tornado:
                return Get("disaster.tornado");
            case DisasterType.Tsunami:
                return Get("disaster.tsunami");
            default:
                return disasterType.ToString();
        }
    }

    public static ModLanguage GetCurrentLanguage()
    {
        return Services.DisasterSetup != null ? Services.DisasterSetup.Language : ModLanguage.English;
    }
}
