
continue https://confluence.kingland.com/display/DHTD/Allow+Parent+Child+Merges

168

WebEx meeting   
https://kingland.webex.com/join/bekle   |  921 327 441     


select * from table where datediff(day,sj,'YYYY-MM-DD') = 0
select *from tablename where adddate>'你需要大于的时间'

select count(*) from table where DATEDIFF ([second], '2004-09-18 00:00:18', '2004-09-18 00:00:19')  > 0

说明

　　select  DATEDIFF(day, time1 , time2)    对应示例语句如下

　　select  DATEDIFF(day, '2010-07-23 0:41:18', '2010-07-23 23:41:18')

　　time1 > time2 为负数;

　　time1 < time2 为正数;

　　[day] :只会比较 2010-07-23 忽略 0:41:18' 其他同理

以下分别: 

　　年: SELECT DATEDIFF([year],time1 , time2)   返回值： -6 ，说明是后减前 与 mysql 相反的。

　　月: SELECT DATEDIFF([month], time1 , time2)

　　天: SELECT DATEDIFF([day], time1 , time2)

　　小时: SELECT DATEDIFF([hour], time1 , time2)

　　秒: SELECT DATEDIFF([second], time1 , time2) 
