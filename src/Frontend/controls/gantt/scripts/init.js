$(document).ready(function()
{
    var ContainerId = "gantt_here";
    var SampleData = GetSampleData();

    gantt.init(ContainerId);

//gantt.parse(SampleData);

    function EnableScroller() {
        // TODO : В плагин JQuery mousewheel вручную включил 'event capture', подумать над альтернативой
        $('body').mousewheel(function(event, delta) {
            gantt.scrollTo(gantt.getScrollState().x - delta * 100);
            event.preventDefault();
        });
    }
	
	EnableScroller();
})

//@Depricated
function GetSampleData()
{
	var Tasks = 
	{
	data: 
	[
		{
			"id": 1, text: "Project #2", start_date: "01-04-2013", duration: 18, order: 10,
			progress: 0.4, open: true
		},
		{
			"id": 2, text: "Task #1", start_date: "02-04-2013", duration: 8, order: 10,
			progress: 0.6, parent: 1
		},
		{
			"id": 3, text: "Task #2", start_date: "11-04-2013", duration: 8, order: 20,
			progress: 0.6, parent: 1
		}
	],
	links: 
	[
		{ id: 1, source: 1, target: 2, type: "1" },
		{ id: 2, source: 2, target: 3, type: "0" },
		{ id: 3, source: 3, target: 4, type: "0" },
		{ id: 4, source: 2, target: 5, type: "2" },
	]
	};
	return Tasks;
}