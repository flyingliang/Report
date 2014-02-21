using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JH.HS.DataExchange._103
{
    class SqlString
    {
        public static string Query1 = @"select student.id
		,sss.健康與體育
		,sss.藝術與人文
		,sss.綜合活動
		,disc.大功支數
,disc.小功支數
,disc.嘉獎支數
,disc.大過支數
,disc.小過支數
,disc.警告支數
,slr.服務學習時數_八上
,slr.服務學習時數_八下
,slr.服務學習時數_九上
from student 
left outer join class on student.ref_class_id=class.id
left outer join 
(
select student.id
			,avg(cast(xpath_string('<root>'||x1.score_info||'</root>','/root/Domains/Domain[@領域=''健康與體育'']/@成績') as float)) as ""健康與體育""
			,avg(cast(xpath_string('<root>'||x1.score_info||'</root>','/root/Domains/Domain[@領域=''藝術與人文'']/@成績') as float)) as ""藝術與人文""
			,avg(cast(xpath_string('<root>'||x1.score_info||'</root>','/root/Domains/Domain[@領域=''綜合活動'']/@成績') as float)) as ""綜合活動""
		from 
			student join class on student.ref_class_id=class.id and class.grade_year = 3
			left join (
								SELECT student.id
	, ''||g1.SchoolYear as schoolyear1
	, ''||g2.SchoolYear as schoolyear2
	, ''||g3.SchoolYear as schoolyear3
	, ''||g4.SchoolYear as schoolyear4
	, ''||g5.SchoolYear as schoolyear5
	, ''||g6.SchoolYear as schoolyear6
FROM student 
left outer join (SELECT id, max(SchoolYear) as SchoolYear FROM xpath_table( 'id', '''<root>''||sems_history||''</root>''', 'student', '/root/History[ ( @GradeYear=''7'' or @GradeYear=''1'' ) and (@Semester=''1'')]/@SchoolYear', 'id IN ( select student.id from student LEFT OUTER JOIN class ON student.ref_class_id = class.id WHERE student.status=1 AND class.grade_year = 3 )') AS tmp(id int, SchoolYear integer) group by id )as g1 on g1.id=student.id left outer join (SELECT id, max(SchoolYear) as SchoolYear FROM xpath_table( 'id', '''<root>''||sems_history||''</root>''', 'student', '/root/History[ ( @GradeYear=''7'' or @GradeYear=''1'' ) and (@Semester=''2'')]/@SchoolYear', 'id IN ( select student.id from student LEFT OUTER JOIN class ON student.ref_class_id = class.id WHERE student.status=1 AND class.grade_year = 3 )') AS tmp(id int, SchoolYear integer) group by id )as g2 on g2.id=student.id left outer join (SELECT id, max(SchoolYear) as SchoolYear FROM xpath_table( 'id', '''<root>''||sems_history||''</root>''', 'student', '/root/History[ ( @GradeYear=''8'' or @GradeYear=''2'' ) and (@Semester=''1'')]/@SchoolYear', 'id IN ( select student.id from student LEFT OUTER JOIN class ON student.ref_class_id = class.id WHERE student.status=1 AND class.grade_year = 3 )') AS tmp(id int, SchoolYear integer) group by id )as g3 on g3.id=student.id left outer join (SELECT id, max(SchoolYear) as SchoolYear FROM xpath_table( 'id', '''<root>''||sems_history||''</root>''', 'student', '/root/History[ ( @GradeYear=''8'' or @GradeYear=''2'' ) and (@Semester=''2'')]/@SchoolYear', 'id IN ( select student.id from student LEFT OUTER JOIN class ON student.ref_class_id = class.id WHERE student.status=1 AND class.grade_year = 3 )') AS tmp(id int, SchoolYear integer) group by id )as g4 on g4.id=student.id left outer join (SELECT id, max(SchoolYear) as SchoolYear FROM xpath_table( 'id', '''<root>''||sems_history||''</root>''', 'student', '/root/History[ ( @GradeYear=''9'' or @GradeYear=''3'' ) and (@Semester=''1'')]/@SchoolYear', 'id IN ( select student.id from student LEFT OUTER JOIN class ON student.ref_class_id = class.id WHERE student.status=1 AND class.grade_year = 3 )') AS tmp(id int, SchoolYear integer) group by id )as g5 on g5.id=student.id left outer join (SELECT id, max(SchoolYear) as SchoolYear FROM xpath_table( 'id', '''<root>''||sems_history||''</root>''', 'student', '/root/History[ ( @GradeYear=''9'' or @GradeYear=''3'' ) and (@Semester=''2'')]/@SchoolYear', 'id IN ( select student.id from student LEFT OUTER JOIN class ON student.ref_class_id = class.id WHERE student.status=1 AND class.grade_year = 3 )') AS tmp(id int, SchoolYear integer) group by id )as g6 on g6.id=student.id LEFT OUTER JOIN class ON student.ref_class_id = class.id WHERE student.status=1 AND class.grade_year = 3)shistory on student.id=shistory.id
			left join sems_subj_score as x1 on student.id=x1.ref_student_id
			and (
				(''||x1.school_year=shistory.schoolyear1 and x1.semester= 1)
				or (''||x1.school_year=shistory.schoolyear2 and x1.semester= 2)
				or (''||x1.school_year=shistory.schoolyear3 and x1.semester= 1)
				or (''||x1.school_year=shistory.schoolyear4 and x1.semester= 2)
				or (''||x1.school_year=shistory.schoolyear5 and x1.semester= 1)
			)
	group by student.id
) as sss on student.id = sss.id
left outer join 
(
	select 
				student.id
				,''||sum(CAST('0'|| xpath_string(x1.detail,'/Discipline/Merit[not(@ Cleared=""是"")]/@A') as integer)) as ""大功支數""
				,''||sum(CAST('0'|| xpath_string(x1.detail,'/Discipline/Merit[not(@ Cleared=""是"")]/@B') as integer)) as ""小功支數""
				,''||sum(CAST('0'|| xpath_string(x1.detail,'/Discipline/Merit[not(@ Cleared=""是"")]/@C') as integer)) as ""嘉獎支數""
				,''||sum(CAST('0'|| xpath_string(x1.detail,'/Discipline/Demerit[not(@ Cleared=""是"")]/@A') as integer)) as ""大過支數""
				,''||sum(CAST('0'|| xpath_string(x1.detail,'/Discipline/Demerit[not(@ Cleared=""是"")]/@B') as integer)) as ""小過支數""
				,''||sum(CAST('0'|| xpath_string(x1.detail,'/Discipline/Demerit[not(@ Cleared=""是"")]/@C') as integer)) as ""警告支數""
			from 
				student join class on student.ref_class_id=class.id and class.grade_year = 3
				left join (
					SELECT student.id
		, ''||g1.SchoolYear as schoolyear1
		, ''||g2.SchoolYear as schoolyear2
		, ''||g3.SchoolYear as schoolyear3
		, ''||g4.SchoolYear as schoolyear4
		, ''||g5.SchoolYear as schoolyear5
		, ''||g6.SchoolYear as schoolyear6
	FROM student 
	left outer join (SELECT id, max(SchoolYear) as SchoolYear FROM xpath_table( 'id', '''<root>''||sems_history||''</root>''', 'student', '/root/History[ ( @GradeYear=''7'' or @GradeYear=''1'' ) and (@Semester=''1'')]/@SchoolYear', 'id IN ( select student.id from student LEFT OUTER JOIN class ON student.ref_class_id = class.id WHERE student.status=1 AND class.grade_year = 3 )') AS tmp(id int, SchoolYear integer) group by id )as g1 on g1.id=student.id left outer join (SELECT id, max(SchoolYear) as SchoolYear FROM xpath_table( 'id', '''<root>''||sems_history||''</root>''', 'student', '/root/History[ ( @GradeYear=''7'' or @GradeYear=''1'' ) and (@Semester=''2'')]/@SchoolYear', 'id IN ( select student.id from student LEFT OUTER JOIN class ON student.ref_class_id = class.id WHERE student.status=1 AND class.grade_year = 3 )') AS tmp(id int, SchoolYear integer) group by id )as g2 on g2.id=student.id left outer join (SELECT id, max(SchoolYear) as SchoolYear FROM xpath_table( 'id', '''<root>''||sems_history||''</root>''', 'student', '/root/History[ ( @GradeYear=''8'' or @GradeYear=''2'' ) and (@Semester=''1'')]/@SchoolYear', 'id IN ( select student.id from student LEFT OUTER JOIN class ON student.ref_class_id = class.id WHERE student.status=1 AND class.grade_year = 3 )') AS tmp(id int, SchoolYear integer) group by id )as g3 on g3.id=student.id left outer join (SELECT id, max(SchoolYear) as SchoolYear FROM xpath_table( 'id', '''<root>''||sems_history||''</root>''', 'student', '/root/History[ ( @GradeYear=''8'' or @GradeYear=''2'' ) and (@Semester=''2'')]/@SchoolYear', 'id IN ( select student.id from student LEFT OUTER JOIN class ON student.ref_class_id = class.id WHERE student.status=1 AND class.grade_year = 3 )') AS tmp(id int, SchoolYear integer) group by id )as g4 on g4.id=student.id left outer join (SELECT id, max(SchoolYear) as SchoolYear FROM xpath_table( 'id', '''<root>''||sems_history||''</root>''', 'student', '/root/History[ ( @GradeYear=''9'' or @GradeYear=''3'' ) and (@Semester=''1'')]/@SchoolYear', 'id IN ( select student.id from student LEFT OUTER JOIN class ON student.ref_class_id = class.id WHERE student.status=1 AND class.grade_year = 3 )') AS tmp(id int, SchoolYear integer) group by id )as g5 on g5.id=student.id left outer join (SELECT id, max(SchoolYear) as SchoolYear FROM xpath_table( 'id', '''<root>''||sems_history||''</root>''', 'student', '/root/History[ ( @GradeYear=''9'' or @GradeYear=''3'' ) and (@Semester=''2'')]/@SchoolYear', 'id IN ( select student.id from student LEFT OUTER JOIN class ON student.ref_class_id = class.id WHERE student.status=1 AND class.grade_year = 3 )') AS tmp(id int, SchoolYear integer) group by id )as g6 on g6.id=student.id LEFT OUTER JOIN class ON student.ref_class_id = class.id WHERE student.status=1 AND class.grade_year = 3
	)shistory on student.id=shistory.id
				left join discipline as x1 on student.id=x1.ref_student_id
				and (
					(''||x1.school_year=shistory.schoolyear1 and x1.semester= 1)
					or (''||x1.school_year=shistory.schoolyear2 and x1.semester= 2)
					or (''||x1.school_year=shistory.schoolyear3 and x1.semester= 1)
					or (''||x1.school_year=shistory.schoolyear4 and x1.semester= 2)
					or (''||x1.school_year=shistory.schoolyear5 and x1.semester= 1)
	                or (''||x1.school_year=shistory.schoolyear6 and x1.semester= 2)
				)
			where 
				student.status = 1 
				and class.grade_year = 3
			group by student.id
)as disc on student.id = disc.id
left outer join 
(
	select student.id
				,''||sum(CASE WHEN (''||x1.school_year=shistory.schoolyear3 and x1.semester= 1) THEN x1.hours ELSE 0 END) as ""服務學習時數_八上""
				,''||sum(CASE WHEN (''||x1.school_year=shistory.schoolyear4 and x1.semester= 2) THEN x1.hours ELSE 0 END) as ""服務學習時數_八下""
				,''||sum(CASE WHEN (''||x1.school_year=shistory.schoolyear5 and x1.semester= 1) THEN x1.hours ELSE 0 END) as ""服務學習時數_九上""
			from 
				student join class on student.ref_class_id=class.id and class.grade_year = 3
				left join (
					SELECT student.id
		, ''||g1.SchoolYear as schoolyear1
		, ''||g2.SchoolYear as schoolyear2
		, ''||g3.SchoolYear as schoolyear3
		, ''||g4.SchoolYear as schoolyear4
		, ''||g5.SchoolYear as schoolyear5
		, ''||g6.SchoolYear as schoolyear6
	FROM student 
	left outer join (SELECT id, max(SchoolYear) as SchoolYear FROM xpath_table( 'id', '''<root>''||sems_history||''</root>''', 'student', '/root/History[ ( @GradeYear=''7'' or @GradeYear=''1'' ) and (@Semester=''1'')]/@SchoolYear', 'id IN ( select student.id from student LEFT OUTER JOIN class ON student.ref_class_id = class.id WHERE student.status=1 AND class.grade_year = 3 )') AS tmp(id int, SchoolYear integer) group by id )as g1 on g1.id=student.id left outer join (SELECT id, max(SchoolYear) as SchoolYear FROM xpath_table( 'id', '''<root>''||sems_history||''</root>''', 'student', '/root/History[ ( @GradeYear=''7'' or @GradeYear=''1'' ) and (@Semester=''2'')]/@SchoolYear', 'id IN ( select student.id from student LEFT OUTER JOIN class ON student.ref_class_id = class.id WHERE student.status=1 AND class.grade_year = 3 )') AS tmp(id int, SchoolYear integer) group by id )as g2 on g2.id=student.id left outer join (SELECT id, max(SchoolYear) as SchoolYear FROM xpath_table( 'id', '''<root>''||sems_history||''</root>''', 'student', '/root/History[ ( @GradeYear=''8'' or @GradeYear=''2'' ) and (@Semester=''1'')]/@SchoolYear', 'id IN ( select student.id from student LEFT OUTER JOIN class ON student.ref_class_id = class.id WHERE student.status=1 AND class.grade_year = 3 )') AS tmp(id int, SchoolYear integer) group by id )as g3 on g3.id=student.id left outer join (SELECT id, max(SchoolYear) as SchoolYear FROM xpath_table( 'id', '''<root>''||sems_history||''</root>''', 'student', '/root/History[ ( @GradeYear=''8'' or @GradeYear=''2'' ) and (@Semester=''2'')]/@SchoolYear', 'id IN ( select student.id from student LEFT OUTER JOIN class ON student.ref_class_id = class.id WHERE student.status=1 AND class.grade_year = 3 )') AS tmp(id int, SchoolYear integer) group by id )as g4 on g4.id=student.id left outer join (SELECT id, max(SchoolYear) as SchoolYear FROM xpath_table( 'id', '''<root>''||sems_history||''</root>''', 'student', '/root/History[ ( @GradeYear=''9'' or @GradeYear=''3'' ) and (@Semester=''1'')]/@SchoolYear', 'id IN ( select student.id from student LEFT OUTER JOIN class ON student.ref_class_id = class.id WHERE student.status=1 AND class.grade_year = 3 )') AS tmp(id int, SchoolYear integer) group by id )as g5 on g5.id=student.id left outer join (SELECT id, max(SchoolYear) as SchoolYear FROM xpath_table( 'id', '''<root>''||sems_history||''</root>''', 'student', '/root/History[ ( @GradeYear=''9'' or @GradeYear=''3'' ) and (@Semester=''2'')]/@SchoolYear', 'id IN ( select student.id from student LEFT OUTER JOIN class ON student.ref_class_id = class.id WHERE student.status=1 AND class.grade_year = 3 )') AS tmp(id int, SchoolYear integer) group by id )as g6 on g6.id=student.id LEFT OUTER JOIN class ON student.ref_class_id = class.id WHERE student.status=1 AND class.grade_year = 3
	)shistory on student.id=shistory.id
				left join $k12.service.learning.record as x1 on (''||student.id)=x1.ref_student_id 
				and (
					(''||x1.school_year=shistory.schoolyear1 and x1.semester= 1)
					or (''||x1.school_year=shistory.schoolyear2 and x1.semester= 2)
					or (''||x1.school_year=shistory.schoolyear3 and x1.semester= 1)
					or (''||x1.school_year=shistory.schoolyear4 and x1.semester= 2)
					or (''||x1.school_year=shistory.schoolyear5 and x1.semester= 1)
				)
			group by student.id
) as slr on student.id = slr.id
where student.status = 1 and class.grade_year = 3";
    }
}