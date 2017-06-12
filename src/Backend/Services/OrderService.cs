using System;
using System.Collections.Generic;
using System.Linq;
using Entities;

namespace Backend
{
    public class OrderService
    {
        public IList<GanttData> GetDetails(Order Order)
        {
            var Result = new List<GanttData>();
            var RootWorks = new List<GanttData>();

            // Корень заказа 
            Order.StartTime = GetStartDateOrder(Order);

            var dbManager = DatabaseManager.GetInstance();
            var request = @"SELECT RootWorks.Duration, Zakaz.NaimZak
                                FROM(SELECT SUM(rw.Duration) AS Duration, rw.id
                                  FROM dbo.RootWorks rw
                                  WHERE rw.id LIKE 'net%'
                                  GROUP BY rw.id) RootWorks
                                INNER JOIN dbo.tempPOSPRIMB p ON RootWorks.id = p.id
                                INNER JOIN dbo.Zakaz ON Zakaz.Zakaz = p.Z
                                GROUP BY RootWorks.Duration, Zakaz.NaimZak";
            var Row = dbManager.SendRequest(request);
            var OrderDuration = Convert.ToInt32(Row[0]["Duration"]);
            Order.Name = Row[0]["NaimZak"].ToString();
            var Root = new GanttData()
            {
                id = 1,
                text = Order.Name,
                //order = 1, // не знаю что за параметр
                start_date = Order.StartTime.Date.ToString(), // Дата открытия заказа, от нее будет считаться все остальное
                duration = OrderDuration,
                open = true,
                progress = 0
            };
            Result.Add(Root);

            ExplodeOrder(Order);

            RootWorks = GetRootWorks(Order);
            Result.AddRange(RootWorks);

            return Result;
        }

        // Вызвать хранимку, которая разузлует заказ и поместит результаты в таблицу tempPOSPRIMB
        public void ExplodeOrder(Order Order)
        {
            var dbManager = DatabaseManager.GetInstance();
            String Request = @"EXEC [1gb_x_t_mes].[dbo].[RAZUZLOVZAKAZ] 'netGraf',@OrderId";
            var RequestArgs = new Dictionary<String, Object>();
            RequestArgs.Add("OrderId", Order.Code);
            //  dbManager.SendCommand(Request,RequestArgs);
        }

        // Получить из базы корневые работы в виде: "Цех:Трудоемкость"
        public List<GanttData> GetRootWorks(Order order)
        {
            var Result = new List<GanttData>();
            var dbManager = DatabaseManager.GetInstance();
            String Request = @"Select   case when (a.tip='Б') then a.id_record else (ROW_NUMBER() over (order by a.id_record)) end as id_record, a.C, a.TIP, a.IND1, a.PICH, a.IND2, a.P2NI, a.Z, a.id, a.Parent,   CASE WHEN a.NV>8 THEN (nv/8)*24 ELSE a.NV END NV ,Depth
                                from
                                (SELECT   tp.id_record, tn.C, tp.TIP, tp.IND1, tp.PICH, tp.IND2, tp.P2NI, tp.Z, tp.id, tp.Parent,   SUM( tn.NV) NV,Depth
                                                                FROM         tempPOSPRIMB AS tp 
                                                                INNER JOIN
                                                                TEXNORM AS tn ON tp.TIP = tn.TIP AND tp.IND1 = tn.IND AND tp.PICH = tn.PICH
                                                                WHERE     (tp.id = 'netgraf')  AND ksz>0
                                GROUP BY tp.id_record, tn.C, tp.TIP, tp.IND1, tp.PICH, tp.IND2, tp.P2NI, tp.Z, tp.id, tp.Parent,Depth
                                   ) as a 
order by depth DESC, CASE WHEN TIP='Б' THEN 1 WHEN TIP='Д' THEN 2 WHEN TIP='Н' THEN 3 WHEN TIP='П' THEN 4 ELSE 5 end,pich,c";
            var Rows = dbManager.SendRequest(Request);
            //var Index = 2;
            var startDurationTime = order.StartTime.Date;
            order.StartTime = GetStartDateOrder(order);
            var StandertWorkToGantt = GetStandartWorks().OrderBy(r => r.id);

            foreach (var item in StandertWorkToGantt)
            {
                if (item.NameWork == "Контракт")
                    startDurationTime = startDurationTime.AddDays(Convert.ToInt16(item.Duration) * -1);
                var ganttStandartWorks = new GanttData
                {
                    id = Convert.ToInt32(item.id),
                    text = item.NameWork,
                    start_date = startDurationTime.ToString(),
                    open = false,
                    duration = Convert.ToDouble(item.Duration),
                    progress = 1,
                    parent=0
                };
                startDurationTime = startDurationTime.AddDays(Convert.ToInt16(item.Duration) / 24);
                Result.Add(ganttStandartWorks);
            }
            var EndDateStandartWork = Convert.ToDateTime(Result.Last().start_date).AddDays(Result.Last().duration/24); 

            var countStandartWorks = Result.Count();
            foreach (var item in Rows)
            {
                if (item["TIP"].ToString() == "Б")
                {
                    var GanttData = new GanttData()
                    {
                        id = Convert.ToInt32(item["id_record"]),
                        text = "(" + item["C"] + ")" + item["PICH"],
                        start_date = startDurationTime.ToString(),
                        //order = 10,
                        open = true,
                        duration = Convert.ToDouble(item["NV"]),
                        parent = Convert.ToInt32(item["Parent"].ToString().Replace("-1", "0")),
                        progress = 0.5
                    };
                    Result.Add(GanttData);
                    startDurationTime = startDurationTime.AddDays(Convert.ToDouble(item["Duration"])).AddDays(1);
                }

            }
            var orderResult = (Result.OrderBy(q => q.start_date)).ToList();
            for (int index = orderResult.Count; index > 0; index--)
            {
                var item = Result[index - 1];
                if (item.parent != -1 && item.parent != 0)
                {
                    var parent = item.parent;
                    var pos = Result.Where(q => q.id == parent).OrderByDescending(w => w.start_date).Select(q => q);
                    var dt = pos.ToList()[0].start_date;
                    var nv = Math.Ceiling(Convert.ToDecimal(item.duration) / 24);

                    var tempDate = ((Convert.ToDateTime(dt).AddDays(-1)).AddDays(Convert.ToInt32(nv) * -1)).ToString();

                    item.start_date = tempDate;
                }
            }
           

          


            var prevPich = "";
            var actPich = "";
            DateTime lastdate = DateTime.Now;
            foreach (var item in Rows)
            {
                if (item["TIP"].ToString() == "Д")
                {
                    actPich = item["PICH"].ToString();
                    var parent = Convert.ToInt64(item["Parent"]);
                    var nv = Math.Ceiling(Convert.ToDouble(item["NV"]));
                    var qwe = nv / 8;
                    var n = (int)qwe + 1;

                    var date = (Convert.ToDateTime(((Result.Where(w => w.id == parent).Select(q => q.start_date)).OrderBy(q => q).Last()))).AddDays(-n);

                    var GanttData = new GanttData()
                    {
                        id = Convert.ToInt32(item["id_record"]),
                        text = "(" + item["C"] + ")" + item["PICH"],
                        start_date = date.ToString(),
                        //order = 10,
                        open = false,
                        duration = Convert.ToDouble(item["NV"]),
                        parent = Convert.ToInt32(item["Parent"].ToString().Replace("-1", "0")),
                        progress = 0.5
                    };
                    Result.Add(GanttData);

                    if (actPich == prevPich)
                    {
                        if (Result.Count() > countStandartWorks + 1)
                        {
                            var nDate = date.AddDays(-1);
                            date = nDate;

                            if (lastdate == date)
                            {
                                nDate = date.AddDays(-1);
                                date = nDate;
                                lastdate = date;
                            }

                            Result.Last().start_date = date.ToString();

                            lastdate = date;
                            var index = Result.Count() - 2;
                            Result[index].duration = 8;
                        }
                    }
                    prevPich = item["PICH"].ToString();
                }
            }
 var startdateWork =
                Result.Where(q => Convert.ToDateTime(q.start_date) > EndDateStandartWork)
                    .OrderBy(w => Convert.ToDateTime(w.start_date)).First();

            var dif = (Convert.ToDateTime(startdateWork.start_date) - EndDateStandartWork).TotalHours;
            var buy = new GanttData
            {
                start_date = EndDateStandartWork.ToString(),
                id = 999999999,
                duration = dif-10,
                open = true,
                text = "Закупка",
                progress = 1,
            };

            var lst = (Result.Take(countStandartWorks));
            var NewResult = new List<GanttData>();
            NewResult.AddRange(lst);
            NewResult.Add(buy);
            NewResult.AddRange(Result.Skip(countStandartWorks+1));
            
            return  NewResult;
        }

        // Получить заказ по номеру
        public Order GetOrderById(Int32 OrderId)
        {
            var Result = new Order();

            var dbManager = DatabaseManager.GetInstance();
            String Request = @"SELECT zakaz, NaimZak
                            FROM [Zakaz]
                            WHERE zakaz = @Order";
            var RequestArgs = new Dictionary<String, Object>();
            RequestArgs.Add("Order", OrderId);
            var Rows = dbManager.SendRequest(Request, RequestArgs);

            if (Rows.Count > 0)
            {
                Result.Code = Rows[0]["zakaz"] as String;
                Result.Name = Rows[0]["NaimZak"] as String;
            }
            return Result;
        }

        public List<Exploder> GetExploderOrder(Int32 order, int parent)
        {
            var result = new List<Exploder>();
            var dbManager = DatabaseManager.GetInstance();
            String Request = @"SELECT id_record, TIP, IND1, PICH, Depth, KSZ 
                            FROM[1gb_x_t_mes].dbo.tempPOSPRIMB WHERE Parent = @Parent and Z=@Z  and id like 'net%' and ksz > 0";
            var RequestArgs = new Dictionary<String, Object>();
            RequestArgs.Add("Parent", parent);
            RequestArgs.Add("Z", order);
            var Rows = dbManager.SendRequest(Request, RequestArgs);

            foreach (var Row in Rows)
            {
                var Exploder = new Exploder
                {
                    id_record = Row["id_record"].ToString(),
                    Type = Row["TIP"].ToString(),
                    Ind = Row["IND1"].ToString(),
                    Denotation = Row["PICH"].ToString(),
                    Depth = Row["Depth"].ToString(),
                    Amount = Row["KSZ"].ToString()
                };
                result.Add(Exploder);
            }

            return result;
        }

        public List<WorkDetail> GetWorkDetails(int idRecord)
        {
            var result = new List<WorkDetail>();

            var dbManager = new DatabaseManager();
            var Request = @"SELECT IND1, PICH, F, C, NV, NAZVO 
                           FROM [1gb_x_t_mes].dbo.BlocksByOperation WHERE id = 'netGraf' AND id_record=@idRecord ";
            var RequestArgs = new Dictionary<String, Object>();
            RequestArgs.Add("idRecord", idRecord);
            var Rows = dbManager.SendRequest(Request, RequestArgs);

            foreach (var Row in Rows)
            {
                var WorkDetail = new WorkDetail
                {
                    Ind = Row["IND1"].ToString(),
                    Denotation = Row["PICH"].ToString(),
                    Chain = Row["F"].ToString(),
                    Department = Row["C"].ToString(),
                    Duration = Row["NV"].ToString(),
                    Operation = Row["NAZVO"].ToString()
                };
                result.Add(WorkDetail);
            }
            return result;

        }

        public DateTime GetStartDateOrder(Order order)
        {
            var dbManager = DatabaseManager.GetInstance();

            var request = @"SELECT TOP 1  do.DDoc DDoc
                    FROM TGInf t
                    INNER JOIN DocOsn do ON t.ID_DocOsn = do.ID_DocOsn
                                  WHERE Z =  @order  ORDER BY t.NS DESC";
            var RequestArgs = new Dictionary<string, object>();
            RequestArgs.Add("order", order.Code);
            var Rows = dbManager.SendRequest(request, RequestArgs);


            var result = Convert.ToDateTime(Rows[0]["DDoc"].ToString());

            return result;
        }

        public List<StandartWorks> GetStandartWorks()
        {
            var result = new List<StandartWorks>();
            var dbManager = new DatabaseManager();
            var request = @"SELECT id,NameWork, Duration FROM [1gb_x_t_mes].dbo.StandartWorks order by id";
            var rows = dbManager.SendRequest(request);

            foreach (var item in rows)
            {
                var StandartWorks = new StandartWorks
                {
                    id = item["id"].ToString(),
                    NameWork = item["NameWork"].ToString(),
                    Duration = item["Duration"].ToString()
                };
                result.Add(StandartWorks);
            }
            return result;
        }

        public void AddStandartWorks( string NameWork, string Duration)
        {
            var dbManager = new DatabaseManager();
            var request = @" insert into [1gb_x_t_mes].dbo.StandartWorks (NameWork, Duration) values ('" + NameWork +
                          "','" + Duration + "' )";
            dbManager.SendRequest(request);
        }

        public void DeleteStandartWorks(int idWorks)
        {
            var dbManager= new DatabaseManager();
            var request = " delete from [1gb_x_t_mes].dbo.StandartWorks  where id=" + idWorks;
            dbManager.SendRequest(request);
        }
    }
}