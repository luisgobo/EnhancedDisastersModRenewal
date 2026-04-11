using System.Collections.Generic;
using ICities;
using NaturalDisastersRenewal.Common.enums;

namespace NaturalDisastersRenewal.Common
{
    public static class LocalizationService
    {
        private static readonly Dictionary<ModLanguage, Dictionary<string, string>> Translations =
            new Dictionary<ModLanguage, Dictionary<string, string>>
            {
                {
                    ModLanguage.English, new Dictionary<string, string>
                    {
                        { "language.english", "English" },
                        { "language.spanish", "Spanish" },
                        { "time.and", "and" },
                        { "time.less_than_one_day", "Less than one day" },
                        { "time.day", "day" },
                        { "time.month", "month" },
                        { "time.year", "year" },
                        { "month.1", "January" }, { "month.2", "February" }, { "month.3", "March" },
                        { "month.4", "April" }, { "month.5", "May" }, { "month.6", "June" },
                        { "month.7", "July" }, { "month.8", "August" }, { "month.9", "September" },
                        { "month.10", "October" }, { "month.11", "November" }, { "month.12", "December" },
                        { "evacuation.manual", "Manual evacuation" },
                        { "evacuation.auto", "Auto evacuation" },
                        { "evacuation.focused", "Focused auto evacuation/release" },
                        { "cracks.none", "No Cracks" },
                        { "cracks.always", "Always Cracks" },
                        { "cracks.by_intensity", "Allow Cracks Based on intensity" },
                        { "panel.title", "Disasters info" },
                        { "panel.population_threshold", "Max population to trigger higher disasters" },
                        { "panel.disaster_header", "Disaster - COY/Max Int" },
                        { "panel.axis.probability", "Probability" },
                        { "panel.axis.max_intensity", "Max intensity" },
                        { "panel.stop_all", "Emergency Button (stop all disasters)" },
                        { "panel.disabled", "Disabled" },
                        { "panel.toggle_button.tooltip", "Extended Disasters (drag by right-click)" },
                        { "panel.drag_panel.tooltip", "Drag by right-click to set the panel position." },
                        { "settings.general", "General" },
                        { "settings.language", "Language" },
                        { "settings.language.tooltip", "Choose the language for panel text and tooltips." },
                        { "settings.disable_follow", "Disable automatic disaster follow when it starts." },
                        { "settings.pause_on_start", "Pause on disaster starts" },
                        { "settings.focused_radius", "Partial evacuation Radius (In meters)" },
                        { "settings.focused_radius.tooltip", "Select the Radius (In meters) for Focused evacuations." },
                        { "settings.max_population", "Max population to trigguer higher disasters." },
                        {
                            "settings.max_population.tooltip",
                            "Select the max population to trigger higher disaster intensity."
                        },
                        { "settings.scale_intensity", "Scale max intensity with population" },
                        {
                            "settings.scale_intensity.tooltip",
                            "Maximum intensity for all disasters is set to the minimum at the beginning of the game and gradually increases as the city grows."
                        },
                        { "settings.record_events", "Record disaster events" },
                        {
                            "settings.record_events.tooltip",
                            "Write out disaster name, date of occurrence, and intencity into Disasters.csv file"
                        },
                        { "settings.show_panel_button", "Show Disasters Panel toggle button" },
                        { "settings.positions", "Button/Panel positions:" },
                        { "settings.reset_button_position", "Reset Button Position" },
                        { "settings.reset_panel_position", "Reset Panel Position" },
                        {
                            "settings.hotkey_placeholder",
                            "Hotkey to display/hide panel info: Shift + D (will be configurable soon)."
                        },
                        { "settings.enable_disasters", "Enable Disasters:" },
                        { "settings.save_options", "Save options" },
                        { "settings.save_default", "Save as default for new games" },
                        { "settings.reset_saved", "Reset to the last saved values" },
                        { "settings.reset_defaults", "Reset to the mod default values" },
                        { "settings.evacuation_mode", "Evacuation Mode: " },
                        { "settings.max_probability", "Max probability" },
                        { "settings.times_per_year", " times per year" },
                        {
                            "settings.forest_fire.max_probability.tooltip",
                            "Occurrence (per year) in case of a long period without rain."
                        },
                        {
                            "settings.forest_fire.warmup.tooltip",
                            "No-rain period during which the probability of Forest Fire increases."
                        },
                        { "settings.season_peak.thunderstorm", "Thunderstorm season peak" },
                        {
                            "settings.thunderstorm.max_probability.tooltip",
                            "Occurrence (per year) in thunderstorm season."
                        },
                        { "settings.season_peak.tornado", "Tornado season peak" },
                        { "settings.rain_factor", "Rain factor" },
                        {
                            "settings.rain_factor.tooltip",
                            "Thunderstorm probability increases by this factor during rain."
                        },
                        {
                            "settings.sinkhole.max_probability.tooltip",
                            "Occurrence (per year) in case of a long period of rain."
                        },
                        { "settings.groundwater_capacity", "Groundwater capacity" },
                        {
                            "settings.groundwater_capacity.tooltip",
                            "Set how fast groundwater fills up during rain and causes a sinkhole to appear."
                        },
                        { "settings.no_tornado_fog", "No Tornado during fog" },
                        { "settings.no_tornado_fog.tooltip", "Tornado does not occur during foggy weather" },
                        { "settings.tornado.max_probability.tooltip", "Occurrence (per year) in Tornado season." },
                        { "settings.enable_tornado_destruction", "Enable tornado destruction" },
                        { "settings.min_tornado_destruction", "Minimal intensity for tornado destruction:" },
                        { "settings.min_tornado_destruction.suffix", " Intensity to start destruction" },
                        { "settings.charge_period", "Charge period" },
                        { "settings.charge_period.years", " years" },
                        {
                            "settings.tsunami.max_probability.tooltip",
                            "Maximum occurrence (per year) after a long period without tsunamis."
                        },
                        {
                            "settings.tsunami.warmup.tooltip",
                            "The probability of tsunami increases to the maximum during this period."
                        },
                        { "settings.warmup_period", "Warmup period" },
                        { "settings.warmup_period.days", " days" },
                        {
                            "settings.earthquake.max_probability.tooltip",
                            "Maximum occurrence (per year) after a long period without earthquakes."
                        },
                        {
                            "settings.earthquake.warmup.tooltip",
                            "The probability of earthquake increases to the maximum during this period."
                        },
                        { "settings.enable_aftershocks", "Enable aftershocks" },
                        {
                            "settings.enable_aftershocks.tooltip",
                            "Several aftershocks may occur after a big earthquake. Aftershocks strike the same place."
                        },
                        { "settings.ground_cracks", "Cracks in the ground:" },
                        {
                            "settings.ground_cracks.tooltip",
                            "Based on selection you can put a crack in the ground, ignoring it or put it based on intensity."
                        },
                        { "settings.min_intensity_cracks", "Min. intensity for cracks" },
                        { "settings.min_intensity_cracks.suffix", " minimal Intensity" },
                        { "settings.min_intensity_cracks.tooltip", "Minimal intensity to see cracks on the ground" },
                        { "settings.enable_long_meteor", "Enable long period (9 years) meteor" },
                        { "settings.enable_medium_meteor", "Enable medium period (5 years) meteor" },
                        { "settings.enable_short_meteor", "Enable short period (2 years) meteor" },
                        {
                            "settings.meteor.max_probability.tooltip",
                            "Maximum occurrence of meteor strike per year per one meteor when it approaches the Earth."
                        },
                        { "tooltip.probability", "Probability: {0}" },
                        { "tooltip.forest_fire.locked", "Not unlocked yet (occurs only outside of your area)." },
                        { "tooltip.not_unlocked", "Not unlocked yet" },
                        { "tooltip.no_disaster_for_another", "No {0} for another {1}" },
                        { "tooltip.recently_occurred", "Decreased because {0} occured recently." },
                        { "tooltip.intensity", "Intensity: {0}" },
                        { "tooltip.low_population", "Decreased because of low population." },
                        { "tooltip.forest_fire.no_during_rain", "No {0} during rain." },
                        {
                            "tooltip.forest_fire.maximum_no_rain",
                            "Maximum because there was no rain for more than {0} days."
                        },
                        { "tooltip.forest_fire.increasing_no_rain", "Increasing because there was no rain for {0}" },
                        { "tooltip.sinkhole.groundwater", "Ground water level {0}%" },
                        { "tooltip.thunderstorm.outside_area", "Not unlocked yet (occurs only outside of your area)." },
                        { "tooltip.thunderstorm.rain_increase", "Increased because of rain." },
                        { "tooltip.tornado.no_during_fog", "No {0} during fog." },
                        { "tooltip.earthquake.aftershocks", "Expect {0} more aftershocks" },
                        { "disaster.earthquake", "Earthquake" },
                        { "disaster.forest_fire", "Forest Fire" },
                        { "disaster.meteor_strike", "Meteor Strike" },
                        { "disaster.sinkhole", "Sinkhole" },
                        { "disaster.thunderstorm", "Thunderstorm" },
                        { "disaster.tornado", "Tornado" },
                        { "disaster.tsunami", "Tsunami" }
                    }
                },
                {
                    ModLanguage.Spanish, new Dictionary<string, string>
                    {
                        { "language.english", "Ingles" },
                        { "language.spanish", "Español" },
                        { "time.and", "y" },
                        { "time.less_than_one_day", "Menos de un dia" },
                        { "time.day", "dia" },
                        { "time.month", "mes" },
                        { "time.year", "ano" },
                        { "month.1", "Enero" }, { "month.2", "Febrero" }, { "month.3", "Marzo" },
                        { "month.4", "Abril" }, { "month.5", "Mayo" }, { "month.6", "Junio" },
                        { "month.7", "Julio" }, { "month.8", "Agosto" }, { "month.9", "Septiembre" },
                        { "month.10", "Octubre" }, { "month.11", "Noviembre" }, { "month.12", "Diciembre" },
                        { "evacuation.manual", "Evacuacion manual" },
                        { "evacuation.auto", "Evacuacion automatica" },
                        { "evacuation.focused", "Evacuacion automatica enfocada/liberacion" },
                        { "cracks.none", "Sin grietas" },
                        { "cracks.always", "Siempre con grietas" },
                        { "cracks.by_intensity", "Permitir grietas segun intensidad" },
                        { "panel.title", "Informacion de desastres" },
                        { "panel.population_threshold", "Poblacion maxima para activar desastres mas fuertes" },
                        { "panel.disaster_header", "Desastre - FOA/Fuerza Max" },
                        { "panel.axis.probability", "Probabilidad" },
                        { "panel.axis.max_intensity", "Fuerza maxima" },
                        { "panel.stop_all", "Boton de emergencia (detener todos los desastres)" },
                        { "panel.disabled", "Desactivado" },
                        { "panel.toggle_button.tooltip", "Desastres extendidos (arrastra con clic derecho)" },
                        { "panel.drag_panel.tooltip", "Arrastra con clic derecho para cambiar la posicion del panel." },
                        { "settings.general", "General" },
                        { "settings.language", "Idioma" },
                        { "settings.language.tooltip", "Elige el idioma de los textos y tooltips." },
                        {
                            "settings.disable_follow",
                            "Desactivar el seguimiento automatico del desastre cuando empieza."
                        },
                        { "settings.pause_on_start", "Pausar cuando comienza un desastre" },
                        { "settings.focused_radius", "Radio de evacuacion parcial (en metros)" },
                        {
                            "settings.focused_radius.tooltip",
                            "Selecciona el radio (en metros) para evacuaciones enfocadas."
                        },
                        { "settings.max_population", "Poblacion maxima para activar desastres mas fuertes." },
                        {
                            "settings.max_population.tooltip",
                            "Selecciona la poblacion maxima para activar desastres de mayor intensidad."
                        },
                        { "settings.scale_intensity", "Escalar intensidad maxima segun poblacion" },
                        {
                            "settings.scale_intensity.tooltip",
                            "La intensidad maxima de todos los desastres se fija al minimo al inicio y aumenta a medida que crece la ciudad."
                        },
                        { "settings.record_events", "Registrar eventos de desastre" },
                        {
                            "settings.record_events.tooltip",
                            "Escribe nombre del desastre, fecha e intensidad en el archivo Disasters.csv file"
                        },
                        { "settings.show_panel_button", "Mostrar boton del panel de desastres" },
                        { "settings.positions", "Posiciones del boton/panel:" },
                        { "settings.reset_button_position", "Restablecer posicion del boton" },
                        { "settings.reset_panel_position", "Restablecer posicion del panel" },
                        {
                            "settings.hotkey_placeholder",
                            "Atajo para mostrar/ocultar panel: Shift + D (sera configurable pronto)."
                        },
                        { "settings.enable_disasters", "Activar desastres:" },
                        { "settings.save_options", "Opciones de guardado" },
                        { "settings.save_default", "Guardar como valor predeterminado para partidas nuevas" },
                        { "settings.reset_saved", "Restablecer a los ultimos valores guardados" },
                        { "settings.reset_defaults", "Restablecer a los valores predeterminados del mod" },
                        { "settings.evacuation_mode", "Modo de evacuacion: " },
                        { "settings.max_probability", "Probabilidad maxima" },
                        { "settings.times_per_year", " veces por ano" },
                        {
                            "settings.forest_fire.max_probability.tooltip",
                            "Ocurrencia por ano en caso de un periodo largo sin lluvia."
                        },
                        {
                            "settings.forest_fire.warmup.tooltip",
                            "Periodo sin lluvia durante el cual aumenta la probabilidad de incendio forestal."
                        },
                        { "settings.season_peak.thunderstorm", "Pico de temporada de tormentas" },
                        {
                            "settings.thunderstorm.max_probability.tooltip",
                            "Ocurrencia por ano durante la temporada de tormentas."
                        },
                        { "settings.season_peak.tornado", "Pico de temporada de tornados" },
                        { "settings.rain_factor", "Factor de lluvia" },
                        {
                            "settings.rain_factor.tooltip",
                            "La probabilidad de tormenta aumenta por este factor durante la lluvia."
                        },
                        {
                            "settings.sinkhole.max_probability.tooltip",
                            "Ocurrencia por ano en caso de un periodo largo de lluvia."
                        },
                        { "settings.groundwater_capacity", "Capacidad del agua subterranea" },
                        {
                            "settings.groundwater_capacity.tooltip",
                            "Define que tan rapido se llena el agua subterranea durante la lluvia y provoca un socavon."
                        },
                        { "settings.no_tornado_fog", "Sin tornados durante niebla" },
                        { "settings.no_tornado_fog.tooltip", "El tornado no ocurre durante clima con niebla" },
                        {
                            "settings.tornado.max_probability.tooltip",
                            "Ocurrencia por ano durante la temporada de tornados."
                        },
                        { "settings.enable_tornado_destruction", "Activar destruccion por tornado" },
                        { "settings.min_tornado_destruction", "Intensidad minima para destruccion por tornado:" },
                        { "settings.min_tornado_destruction.suffix", " Intensidad para iniciar destruccion" },
                        { "settings.charge_period", "Periodo de carga" },
                        { "settings.charge_period.years", " anos" },
                        {
                            "settings.tsunami.max_probability.tooltip",
                            "Ocurrencia maxima por ano despues de un largo periodo sin tsunamis."
                        },
                        {
                            "settings.tsunami.warmup.tooltip",
                            "La probabilidad del tsunami aumenta al maximo durante este periodo."
                        },
                        { "settings.warmup_period", "Periodo de calentamiento" },
                        { "settings.warmup_period.days", " dias" },
                        {
                            "settings.earthquake.max_probability.tooltip",
                            "Ocurrencia maxima por ano despues de un largo periodo sin terremotos."
                        },
                        {
                            "settings.earthquake.warmup.tooltip",
                            "La probabilidad del terremoto aumenta al maximo durante este periodo."
                        },
                        { "settings.enable_aftershocks", "Activar replicas" },
                        {
                            "settings.enable_aftershocks.tooltip",
                            "Pueden ocurrir varias replicas despues de un gran terremoto. Las replicas golpean la misma zona."
                        },
                        { "settings.ground_cracks", "Grietas en el suelo:" },
                        {
                            "settings.ground_cracks.tooltip",
                            "Segun la seleccion, puedes crear grietas en el suelo, ignorarlas o activarlas por intensidad."
                        },
                        { "settings.min_intensity_cracks", "Intensidad minima para grietas" },
                        { "settings.min_intensity_cracks.suffix", " Intensidad minima" },
                        { "settings.min_intensity_cracks.tooltip", "Intensidad minima para ver grietas en el suelo" },
                        { "settings.enable_long_meteor", "Activar meteorito de periodo largo (9 anos)" },
                        { "settings.enable_medium_meteor", "Activar meteorito de periodo medio (5 anos)" },
                        { "settings.enable_short_meteor", "Activar meteorito de periodo corto (2 anos)" },
                        {
                            "settings.meteor.max_probability.tooltip",
                            "Ocurrencia maxima por ano de impacto de meteorito por cada meteorito cuando se acerca a la Tierra."
                        },
                        { "tooltip.probability", "Probabilidad: {0}" },
                        { "tooltip.forest_fire.locked", "Aun no desbloqueado (solo ocurre fuera de tu area)." },
                        { "tooltip.not_unlocked", "Aun no desbloqueado" },
                        { "tooltip.no_disaster_for_another", "No habra {0} por otros {1}" },
                        { "tooltip.recently_occurred", "Bajo porque {0} ocurrio recientemente." },
                        { "tooltip.intensity", "Intensidad: {0}" },
                        { "tooltip.low_population", "Disminuido por baja poblacion." },
                        { "tooltip.forest_fire.no_during_rain", "No habra {0} durante la lluvia." },
                        { "tooltip.forest_fire.maximum_no_rain", "Maxima porque no ha llovido por mas de {0} dias." },
                        { "tooltip.forest_fire.increasing_no_rain", "Aumentando porque no ha llovido durante {0}" },
                        { "tooltip.sinkhole.groundwater", "Nivel de agua subterranea {0}%" },
                        { "tooltip.thunderstorm.outside_area", "Aun no desbloqueado (solo ocurre fuera de tu area)." },
                        { "tooltip.thunderstorm.rain_increase", "Aumentado por la lluvia." },
                        { "tooltip.tornado.no_during_fog", "No habra {0} durante la niebla." },
                        { "tooltip.earthquake.aftershocks", "Se esperan {0} replicas mas" },
                        { "disaster.earthquake", "Terremoto" },
                        { "disaster.forest_fire", "Incendio forestal" },
                        { "disaster.meteor_strike", "Meteorito" },
                        { "disaster.sinkhole", "Socavon" },
                        { "disaster.thunderstorm", "Tormenta electrica" },
                        { "disaster.tornado", "Tornado" },
                        { "disaster.tsunami", "Tsunami" }
                    }
                }
            };

        public static string Get(string key)
        {
            var language = GetCurrentLanguage();
            Dictionary<string, string> languageValues;
            if (Translations.TryGetValue(language, out languageValues) && languageValues.ContainsKey(key))
                return languageValues[key];

            return Translations[ModLanguage.English].ContainsKey(key) ? Translations[ModLanguage.English][key] : key;
        }

        public static string Format(string key, params object[] args)
        {
            return string.Format(Get(key), args);
        }

        public static string[] GetMonths()
        {
            return new[]
            {
                Get("month.1"), Get("month.2"), Get("month.3"), Get("month.4"),
                Get("month.5"), Get("month.6"), Get("month.7"), Get("month.8"),
                Get("month.9"), Get("month.10"), Get("month.11"), Get("month.12")
            };
        }

        public static string[] GetLanguageDisplayNames()
        {
            return new[] { Get("language.english"), Get("language.spanish") };
        }

        public static string GetDisasterName(DisasterType disasterType)
        {
            switch (disasterType)
            {
                case DisasterType.Earthquake: return Get("disaster.earthquake");
                case DisasterType.ForestFire: return Get("disaster.forest_fire");
                case DisasterType.MeteorStrike: return Get("disaster.meteor_strike");
                case DisasterType.Sinkhole: return Get("disaster.sinkhole");
                case DisasterType.ThunderStorm: return Get("disaster.thunderstorm");
                case DisasterType.Tornado: return Get("disaster.tornado");
                case DisasterType.Tsunami: return Get("disaster.tsunami");
                default: return disasterType.ToString();
            }
        }

        public static ModLanguage GetCurrentLanguage()
        {
            return Services.DisasterSetup != null ? Services.DisasterSetup.Language : ModLanguage.English;
        }
    }
}