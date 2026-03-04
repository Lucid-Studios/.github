(defparameter *sli-engine-base*
  (make-pathname :name nil :type nil :defaults *load-pathname*))

(load (merge-pathnames "packet_parser.lisp" *sli-engine-base*))
(load (merge-pathnames "packet_validator.lisp" *sli-engine-base*))
(load (merge-pathnames "packet_router.lisp" *sli-engine-base*))

(defpackage :oan-sli.engine
  (:use :cl)
  (:import-from :oan-sli.packet-parser :parse-packet :normalize-packet)
  (:import-from :oan-sli.packet-validator :validate-packet)
  (:import-from :oan-sli.packet-router :route-packet)
  (:export :process-packet-string :main))

(in-package :oan-sli.engine)

(defun %sanitize-input (raw)
  (string-trim
   '(#\Space #\Tab #\Newline #\Return)
   (coerce (remove #\Null raw) 'string)))

(defun %read-stdin ()
  (with-output-to-string (buffer)
    (loop for ch = (read-char *standard-input* nil nil)
          while ch
          do (write-char ch buffer))))

(defun %read-packet-source ()
  (let ((argv sb-ext:*posix-argv*))
    (if (> (length argv) 3)
        (nth 3 argv)
        (%read-stdin))))

(defun %emit-result (result)
  (princ (with-output-to-string (out)
           (prin1 result out))))

(defun process-packet-string (raw)
  (handler-case
      (multiple-value-bind (form position)
          (read-from-string raw)
        (declare (ignore position))
        (when (stringp form)
          (setf form (read-from-string form)))
        (let* ((parsed (parse-packet form))
               (normalized (normalize-packet parsed))
               (validation (validate-packet normalized)))
          (if (getf validation :valid)
              (list :result
                    :status :accepted
                    :route (route-packet normalized)
                    :packet normalized)
              (list :result
                    :status :rejected
                    :validation validation
                    :packet normalized))))
    (error (condition)
      (list :result :status :error :reason (princ-to-string condition)))))

(defun main ()
  (let ((input (%sanitize-input (%read-packet-source))))
    (%emit-result (process-packet-string input))))

(main)
