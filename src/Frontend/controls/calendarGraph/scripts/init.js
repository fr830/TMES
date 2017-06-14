var tE, tW, RequestedOrder, stWork;

$(document).ready(function () {

    $('#ExploderTable thead tr:eq(1) th').each(function () {
        var title = $('#ExploderTable thead tr:eq(0) th').eq($(this).index()).text();
        $(this).html('<input type="text" placeholder="Search ' + title + '" />');
    });

    tE = $('#ExploderTable').DataTable({
        "bPaginate": false,
        "bInfo": false,
        orderCellsTop: true,
        "columnDefs": [
             {
                 "className": "hidden",
                 "targets": [0]
             }
        ]
    });

    tE.columns().every(function (index) {
        $('#ExploderTable thead tr:eq(1) th:eq(' + index + ') input').on('keyup change', function () {
            tE.column($(this).parent().index() + ':visible')
                .search(this.value)
                .draw();
        });
    });

    $('.dataTables_filter').hide();

    tW = $('#WorkDetaleTable').dataTable({
        "bPaginate": false,
        "bInfo": false,
        "bFilter": false,
        "columns":
        [
            { "data": "Ind" },
            { "data": "Denotation" },
            { "data": "Chain" },
            { "data": "Department" },
            { "data": "Duration" },
            { "data": "Operation" }
        ],
        "order": [[2, "asc"]]
    });

    tE.on('click', 'tbody tr', function () {
        var field = $('td', this);

        var idRecord = field.eq(0).text();
        var ind = field.eq(2).text();
        var denotation = field.eq(3).text();

        var dv = $('#Navigate');
        dv.append("<span class='nv' style='background-color: #757c81; cursor:pointer' id=" + idRecord + ">" + ind + " " + denotation + "</span>      ");

        AjaxRequest("api/order/" + RequestedOrder + "/" + idRecord + "/Exploder", null, Exploder);
        AjaxRequest("api/Order/" + idRecord + "/WorkDetail", null, WorkDetail);

        $('.nv').on('click', function () {

            var item = $(this);
            idRecord = item.attr('id');
            AjaxRequest("api/order/" + RequestedOrder + "/" + idRecord + "/Exploder", null, Exploder);
            AjaxRequest("api/Order/" + idRecord + "/WorkDetail", null, WorkDetail);
            for (var i = 0; i < 17; i++) {
                console.log(item.next('span').remove());
            }
        });
    });

    stWork = $('#stWork').dataTable({
        "bPaginate": false,
        "bInfo": false,
        "bFilter": false,
        
         "columnDefs": [
             {
                 "className": "hidden",
                 "targets": [0]
             }
        ]
    });

    document.getElementById('addStandartWork').addEventListener('click', addStandartWork);

    AjaxRequest("api/order/StandartWorks", null, AddStanartWork);
});



function Build() {
    var resultChecked;
    if (document.getElementById('checkBoxStandartWorks').checked === true)
        resultChecked = 1;
    else
        resultChecked = 0;

    RequestedOrder = document.getElementById("RequestedOrderInput").value;
    // AjaxRequest("api/order/" + RequestedOrder, null, ValidateOrder);
    AjaxRequest("api/order/" + RequestedOrder + "/-1" + "/Exploder", null, Exploder);
    AjaxRequest("api/order/" + RequestedOrder + "/" + resultChecked + "/build", null, ShowDetails);

}

function Options() {
    $("#OptionsDialog").dialog({
        width: 500,
        // height: auto,
        modal: true
    });
}
function ValidateOrder(result) {
    //alert(result.Name);
}

function ShowDetails(result) {
    var data = JSON.stringify({ data: result });
    gantt.parse(data);
}

function Exploder(result) {
    //console.log(result);
    tE.clear().draw();
    if (result.length !== 0) {
        for (var i = 0; i < result.length; i++) {
            tE.row.add([result[i].id_record, result[i].Type, result[i].Ind, result[i].Denotation, result[i].Amount, result[i].Depth]).draw();
        }
    }
}

function WorkDetail(result) {
    tW.fnClearTable();
    if (result.length !== 0)
        tW.fnAddData(result);
}

function AddStanartWork(result) {

    stWork.fnClearTable();
    if (result.length !== 0) {
        for (var i = 0; i < result.length; i++) {
            stWork.fnAddData([result[i].id, "<span onclick='DeleteStWork(" + result[i].id + ")' >Delete</span>", result[i].NameWork, result[i].Duration]);
        }
    }
}

function addStandartWork() {
    var naim = $("#stWorkNaim").val();
    var duration = $("#stWorkDuration").val();
    var match = duration.match(/^[0-9]+$/);
    console.log(match);
    if (naim !== "" && duration !== "" && Array.isArray(match) === true) {

        AjaxRequest("api/order/" + naim + "/" + duration + "/AddStandartWorks", null, function() {
            AjaxRequest("api/order/StandartWorks", null, AddStanartWork); 
        });

    } else {
        alert("The data is not valid");
    }
}

function DeleteStWork(id) {
    if (confirm("Delete?")) {
        AjaxRequest("api/order/" + id + "/DeleteStandartWorks", null, function() {
            AjaxRequest("api/order/StandartWorks", null, AddStanartWork);
        });
    }
}