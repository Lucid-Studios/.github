(defpackage :oan-sli.packet-router
  (:use :cl :oan-sli.packet-parser)
  (:export :route-packet))

(in-package :oan-sli.packet-router)

(defun %symbol-in-set-p (value names)
  (and (symbolp value)
       (member (symbol-name value) names :test #'string-equal)))

(defun route-packet (packet)
  (let ((env (packet-get packet :env))
        (frame (packet-get packet :frame)))
    (cond
      ((or (%symbol-in-set-p env '("RUNTIME" "CRADLE" "SOULFRAME" "AGENTICORE"))
           (%symbol-in-set-p frame '("CRADLE" "SOULFRAME" "AGENTICORE")))
       :runtime)
      ((%symbol-in-set-p env '("TELEMETRY" "AUDIT"))
       :telemetry)
      ((%symbol-in-set-p env '("GEL" "LEDGER"))
       :ledger)
      ((%symbol-in-set-p env '("DATA" "CRYPTIC"))
       :data)
      (t
       :runtime))))
