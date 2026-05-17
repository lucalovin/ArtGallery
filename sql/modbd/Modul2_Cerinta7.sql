------------------------------------------------------------
cerinta 7
------------------------------------------------------------

SELECT  
    e.title AS exhibition_title,  
    ex.name AS exhibitor_name,  
    ex.city AS exhibitor_city,  
    COUNT(DISTINCT ax.artwork_id) AS artworks_count,  
    ROUND(AVG(gr.rating), 2) AS average_rating,  
    SUM(a.estimated_value) AS total_estimated_value  
FROM EXHIBITION e  
JOIN EXHIBITOR ex  
    ON ex.exhibitor_id = e.exhibitor_id  
JOIN ARTWORK_EXHIBITION ax  
    ON ax.exhibition_id = e.exhibition_id  
JOIN ARTWORK a  
    ON a.artwork_id = ax.artwork_id  
LEFT JOIN GALLERY_REVIEW gr  
    ON gr.exhibition_id = e.exhibition_id  
GROUP BY  
    e.title,  
    ex.name,  
    ex.city  
ORDER BY total_estimated_value DESC; 


 
ALTER SESSION SET OPTIMIZER_MODE = RULE; 



EXPLAIN PLAN FOR  
SELECT  
    e.title AS exhibition_title,  
    ex.name AS exhibitor_name,  
    ex.city AS exhibitor_city,  
    COUNT(DISTINCT ax.artwork_id) AS artworks_count,  
    ROUND(AVG(gr.rating), 2) AS average_rating,  
    SUM(a.estimated_value) AS total_estimated_value  
FROM EXHIBITION e  
JOIN EXHIBITOR ex  
    ON ex.exhibitor_id = e.exhibitor_id  
JOIN ARTWORK_EXHIBITION ax  
    ON ax.exhibition_id = e.exhibition_id  
JOIN ARTWORK a  
    ON a.artwork_id = ax.artwork_id  
LEFT JOIN GALLERY_REVIEW gr  
    ON gr.exhibition_id = e.exhibition_id  
GROUP BY  
    e.title,  
    ex.name,  
    ex.city  
ORDER BY total_estimated_value DESC;  


SELECT * FROM TABLE(DBMS_XPLAN.DISPLAY); 


 

ALTER SESSION SET OPTIMIZER_MODE = ALL_ROWS; 




EXPLAIN PLAN FOR  
SELECT  
        e.title AS exhibition_title,  
        ex.name AS exhibitor_name,  
        ex.city AS exhibitor_city,  
        COUNT(DISTINCT ax.artwork_id) AS artworks_count,  
    ROUND(AVG(gr.rating), 2) AS average_rating,  
    SUM(a.estimated_value) AS total_estimated_value  
FROM EXHIBITION e  
JOIN EXHIBITOR ex  
        ON ex.exhibitor_id = e.exhibitor_id  
JOIN ARTWORK_EXHIBITION ax  
        ON ax.exhibition_id = e.exhibition_id  
JOIN ARTWORK a  
        ON a.artwork_id = ax.artwork_id  
LEFT JOIN GALLERY_REVIEW gr  
        ON gr.exhibition_id = e.exhibition_id  
GROUP BY  
        e.title,  
        ex.name,  
        ex.city  
ORDER BY  
        total_estimated_value DESC;  

   

SELECT * FROM TABLE(DBMS_XPLAN.DISPLAY); 




EXPLAIN PLAN FOR  
SELECT  
    e.title AS exhibition_title,  
    ex.name AS exhibitor_name,  
    ex.city AS exhibitor_city,  
    COUNT(DISTINCT ax.artwork_id) AS artworks_count,  
    ROUND(AVG(gr.rating), 2) AS average_rating,  
    SUM(a.estimated_value) AS total_estimated_value  
FROM EXHIBITION e  
JOIN EXHIBITOR ex  
    ON ex.exhibitor_id = e.exhibitor_id  
JOIN ARTWORK_EXHIBITION ax  
    ON ax.exhibition_id = e.exhibition_id  
JOIN ARTWORK a  
    ON a.artwork_id = ax.artwork_id  
LEFT JOIN GALLERY_REVIEW gr  
    ON gr.exhibition_id = e.exhibition_id  
WHERE e.start_date >= DATE '2026-01-01'  
  AND e.start_date < DATE '2026-05-01'  
GROUP BY  
    e.title,  
    ex.name,  
    ex.city  
ORDER BY total_estimated_value DESC;  

  

SELECT * FROM TABLE(DBMS_XPLAN.DISPLAY); 

 