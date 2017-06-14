$(document).ready(function () {
    var ContainerId = "gantt_here";

    gantt.config.duration_unit = "hour";
    gantt.config.grid_width = 470;
    gantt.config.time_step = 7;
    gantt.init(ContainerId);
    gantt.locale = {
        date: {
            month_full: ["Январь", "Февраль", "Март", "Апрель", "Мая", "Июнь", "Июль",
            "Август", "Сентябрь", "Октябрь", "Ноябрь", "Декабрь"],
            month_short: ["Янв", "Фев", "Март", "Апр", "Май", "Июнь", "Июль", "Авг", "Сент",
            "Окт", "Нояб", "Дек"],
            day_full: ["Воскресенье", "Понедельник", "Вторник", "Среда", "Четверг", "Пятница",
            "Суббота"],
            day_short: ["Вс", "Пн", "Вт", "Ср", "Чт", "Пт", "Сб"]
        },
        labels: {
            new_task: "New",
            icon_save: "Save",
            icon_cancel: "Cansel",
            icon_details: "Детали",
            icon_edit: "Изменить",
            icon_delete: "Удалить",
            confirm_closing: "",//Your changes will be lost, are you sure ?
            confirm_deleting: "Task will be deleted permanently, are you sure?",

            section_description: "Описание",
            section_time: "Период",
            column_duration: "Длительность",
            column_start_date: "Нач. дата",
            column_text: "Сборка",
            /* link confirmation */

            confirm_link_deleting: "Dependency will be deleted permanently, are you sure?",
            link_from: "From",
            link_to: "To",
            link_start: "Start",
            link_end: "End",

            minutes: "Минуты",
            hours: "Часы",
            days: "Дни",
            weeks: "Неделя",
            months: "Месяц",
            years: "Год"
        }
    };

    function EnableScroller() {
        // TODO : В плагин JQuery mousewheel вручную включил 'event capture', подумать над альтернативой
        $('body').mousewheel(function (event, delta) {
            gantt.scrollTo(gantt.getScrollState().x - delta * 100);
            //gantt.scrollTo(gantt.getScrollState().y - delta * 100);
            event.preventDefault();
        });
    }

    EnableScroller();
});

//@Depricated
function GetSampleData() {

}