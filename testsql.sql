			'select a.id, a.tender_order_id, a.tender_order_doc_type_id, a.protocol_number, a.card_number, ' + 
			'    a.tender_date, a.period, a.status, a.note, a.order_receipt_date, a.execute_user_name, a.coexecute_user_name, ' + 
			'    a.execute_user_id, a.coexecute_user_id, a.order_number, a.order_date, a.order_name, a.declarer_name, ' + 
			'    a.order_type_name, a.order_kind_name, a.privat_last_action, a.grd_last_action, ' + 
			'    a.have_comments, a.in_work_date, a.send_privat_date, a.agreed_privat_date, ' + 
			'    a.send_grd_date, a.agreed_grd_date, ' + 
			'    (case ' + 
			'        when coalesce(a.header_winner, '''') = '''' then a.winner ' + 
			'        else a.header_winner ' + 
			'    end) as winner, ' + 
			'    (case ' + 
			'        when coalesce(a.tender_header_sum, 0) = 0 then (a.sum_with_nds + a.add_cost)  ' + 
			'        else a.tender_header_sum ' + 
			'    end) as tender_sum ' + 
			'from ' + 
			'    (SELECT ' + 
			'        th.id, th.quarter, th.tender_year, th.tender_order_id, th.tender_order_doc_type_id, th.protocol_number, th.card_number, th.tender_date, th.period, ' + 
			'        th.status, th.note, th.receipt_date as order_receipt_date, ' + 
			'        (select first 1 tu.name from tender_users tu where tu.id = th.execute_user_id and tu.is_deleted = 0) as execute_user_name,          ' + 
			'        (select first 1 tu.name from tender_users tu where tu.id = th.coexecute_user_id and tu.is_deleted = 0) as coexecute_user_name,          ' + 
			'        th.execute_user_id, th.coexecute_user_id,          ' + 
			'        th.winner as header_winner, th.tender_sum as tender_header_sum, ' + 
			'        decode(th.tender_order_doc_type_id,        ' + 
			'            2,       ' + 
			'                (select       ' + 
			'                    (trim(cast(t.code as varchar(4))) || ''/'' || trim(cast(dot.num_pp as varchar(4))) || ''/'' ||     ' + 
			'                    trim(cast(dot.k_num as varchar(1))) || ''/'' || trim(cast(dot.k_year as varchar(4))))    ' + 
			'                from doc_ord_th dot       ' + 
			'                    join tender_ord t on t.id = dot.id_ot and t.is_del = 0       ' + 
			'                where th.tender_order_id = dot.id and dot.is_del = 0),       ' + 
			'            3,       ' + 
			'                (select t.number from tender_order t where t.id = th.tender_order_id and t.is_deleted = 0)) as order_number,       ' + 
			'        decode(th.tender_order_doc_type_id,       ' + 
			'            2,        ' + 
			'                (select dot.ot_date from doc_ord_th dot where th.tender_order_id = dot.id and dot.is_del = 0),       ' + 
			'            3,        ' + 
			'                (select t.order_date from tender_order t where t.id = th.tender_order_id and t.is_deleted = 0)) as order_date,        ' + 
			'        th.theme as order_name,     ' + 
			'        decode(th.tender_order_doc_type_id,        ' + 
			'            2,        ' + 
			'                (select first 1 tdd.name from tender_department_declarer tdd where tdd.is_upp = 1),     ' + 
			'            3,       ' + 
			'                (select tdd.name from tender_order t     ' + 
			'                    join tender_department_declarer tdd on tdd.id = t.declarer_id     ' + 
			'                 where t.id = th.tender_order_id and t.is_deleted = 0)) as declarer_name,     ' + 
			'        decode(th.tender_order_doc_type_id,        ' + 
			'            2,        ' + 
			'                (select first 1 tot.name from tender_order_type tot where tot.id = 1),     ' + 
			'            3,        ' + 
			'                (select tot.name from tender_order t    ' + 
			'                    join tender_order_type tot on tot.id = t.order_type_id     ' + 
			'                 where t.id = th.tender_order_id and t.is_deleted = 0)) as order_type_name,     ' + 
			'        decode(th.tender_order_doc_type_id,        ' + 
			'            2,        ' + 
			'                (select first 1 tok.name from tender_order_kind tok where tok.id = 1),     ' + 
			'            3,        ' + 
			'                (select tok.name from tender_order t     ' + 
			'                    join tender_order_kind tok on tok.id = t.order_kind_id    ' + 
			'                 where t.id = th.tender_order_id and t.is_deleted = 0)) as order_kind_name,    ' + 
			'        (select first 1 thist.action_id          ' + 
			'        from tender_history thist           ' + 
			'        where           ' + 
			'            thist.doc_id = th.id and thist.doc_type_id = 1 and thist.action_id in (7, 8, 9, 14) and           ' + 
			'            cast(thist.action_date ||''   ''|| thist.action_time as timestamp) >     ' + 
			'                coalesce(           ' + 
			'                    (select first 1 cast(thist2.action_date ||''   ''|| thist2.action_time as timestamp)    ' + 
			'                     from tender_history thist2           ' + 
			'                     where           ' + 
			'                        thist2.doc_id = th.id and           ' + 
			'                        thist2.doc_type_id = 1 and           ' + 
			'                        thist2.action_id in (9, 12, 18, 21)          ' + 
			'                     order by thist2.action_date desc, thist2.action_time desc),           ' + 
			'                     cast(''01.01.1900'' as timestamp))     ' + 
			'        order by thist.action_date desc, thist.action_time desc) as privat_last_action,          ' + 
			'        (select first 1 thist.action_id           ' + 
			'        from tender_history thist           ' + 
			'        where           ' + 
			'            thist.doc_id = th.id and thist.doc_type_id = 1 and thist.action_id in (10, 11, 12, 15) and          ' + 
			'            cast(thist.action_date ||''   ''|| thist.action_time as timestamp) >     ' + 
			'                coalesce(           ' + 
			'                    (select first 1 cast(thist2.action_date ||''   ''|| thist2.action_time as timestamp)     ' + 
			'                     from tender_history thist2          ' + 
			'                     where           ' + 
			'                        thist2.doc_id = th.id and           ' + 
			'                        thist2.doc_type_id = 1 and           ' + 
			'                        thist2.action_id in (9, 12, 18, 21)          ' + 
			'                     order by thist2.action_date desc, thist2.action_time desc),          ' + 
			'                     cast(''01.01.1900'' as timestamp))    ' + 
			'        order by thist.action_date desc, thist.action_time desc) as grd_last_action,          ' + 
			'        (select first 1 tc.id from tender_comments tc where tc.doc_id = th.id and tc.doc_type_id = :doc_type_id and tc.is_deleted = 0) as have_comments,      ' + 
			'        (select first 1 thist.action_date from tender_history thist where thist.doc_id = th.id and thist.action_id = 1 and thist.doc_type_id = :doc_type_id order by thist.action_date desc) as in_work_date,  ' + 
			'        (select first 1 thist.action_date from tender_history thist where thist.doc_id = th.id and thist.action_id = 7 and thist.doc_type_id = :doc_type_id order by thist.action_date desc) as send_privat_date,  ' + 
			'        (select first 1 thist.action_date from tender_history thist where thist.doc_id = th.id and thist.action_id = 8 and thist.doc_type_id = :doc_type_id order by thist.action_date desc) as agreed_privat_date,  ' + 
			'        (select first 1 thist.action_date from tender_history thist where thist.doc_id = th.id and thist.action_id = 10 and thist.doc_type_id = :doc_type_id order by thist.action_date desc) as send_grd_date,  ' + 
			'        (select first 1 thist.action_date from tender_history thist where thist.doc_id = th.id and thist.action_id = 11 and thist.doc_type_id = :doc_type_id order by thist.action_date desc) as agreed_grd_date, ' + 
			'        ts.id as tender_supplier_id, ts.name as winner, ' + 
			'        coalesce(ts.credit_cost, 0) + coalesce(ts.custom_cost, 0) + coalesce(ts.shipping_cost, 0) as add_cost, ' + 
			'        sum( ' + 
			'            case ' + 
			'                when coalesce(tso.amount, 0) <> 0 then ' + 
			'                    cast(round(cast(round(gtp.price * ts.exchange_rate, 2) as double precision) * tso.amount, 2) + ' + 
			'                        round(round(cast(round(gtp.price * ts.exchange_rate, 2) as double precision) * tso.amount, 2) * tp.nds * ts.nds_pay * cast(g.nds as double precision), 2) as numeric(18,2)) ' + 
			'                else ' + 
			'                    cast(round(cast(round(gtp.price * ts.exchange_rate, 2) as double precision) * tp.order_amount, 2) + ' + 
			'                        round(round(cast(round(gtp.price * ts.exchange_rate, 2) as double precision) * tp.order_amount, 2) * tp.nds * ts.nds_pay * cast(g.nds as double precision), 2) as numeric(18,2)) ' + 
			'            end ' + 
			'        ) as sum_with_nds ' +
			'     ' + 
			'    FROM TENDER_HEADER th ' + 
			'        join get_nds_year(:year) g on 1 = 1 ' + 
			'        left join tender_position tp on tp.tender_header_id = th.id and tp.is_deleted = 0 ' + 
			'        left join tender_supplier ts on ts.tender_header_id = th.id and ts.is_deleted = 0 ' + 
			'        left join tender_supplier_offer tso on tso.tender_position_id = tp.id and tso.tender_supplier_id = ts.id and tso.is_winner = 1 ' + 
			'        left join get_tender_price(tso.price, tso.price2, tso.price3) gtp on 1 = 1 ' + 
			'    where ' +
			'        not(tso.offer_sum is null) or (ts.id is null) or ' +
			'        coalesce((select count(tso.tender_supplier_id) from tender_supplier_offer tso ' +
			'            join tender_position tp on tp.tender_header_id = th.id ' +
			'            join tender_supplier ts on ts.tender_header_id = th.id ' +
			'        where tso.tender_position_id = tp.id and tp.is_deleted = 0 and tso.tender_supplier_id = ts.id and ts.is_deleted = 0 and tso.is_winner = 1), 0) = 0 ' +
			'    group by ' + 
			'        1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35) a ' + 
			'where ' + 
			'    a.quarter = :quarter and a.tender_year = :year ';